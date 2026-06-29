using Microsoft.EntityFrameworkCore;
using TrackNGo.Data;
using TrackNGo.Models;
using TrackNGo.ViewModels;

namespace TrackNGo.Services
{
    public interface IDocumentService
    {
        Task<string> GenerateTrackingNumberAsync();
        Task<Document> CreateDocumentAsync(DocumentCreateViewModel model, int createdByUserId);
        Task<Document?> GetByIdAsync(int id);
        Task<Document?> GetByTrackingNumberAsync(string trackingNumber);
        Task<DocumentListViewModel> GetDocumentListAsync(string? search, string? docType,
            string? department, string? status, int page = 1, int pageSize = 12);
        Task<DocumentDetailViewModel> GetDocumentDetailAsync(int id, UserRole userRole);
        Task UpdateDocumentAsync(Document document);
        Task<int> GetTotalCountAsync();
        Task<Dictionary<string, int>> GetStatusDistributionAsync();
        Task<Dictionary<string, int>> GetDepartmentActivityAsync();
        Task<List<string>> GetDocumentTypesAsync();
        Task<List<DepartmentDropdownItem>> GetDepartmentOptionsAsync();
        Task<int> GetCountByStatusAsync(params DocumentStatus[] statuses);
        Task<int> GetReceivedTodayCountAsync();
    }

    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _db;

        public DocumentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateTrackingNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastDoc = await _db.Documents
                .Where(d => d.TrackingNumber.StartsWith($"TNG-{year}-"))
                .OrderByDescending(d => d.TrackingNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastDoc != null)
            {
                var parts = lastDoc.TrackingNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastNum))
                    nextNumber = lastNum + 1;
            }

            return $"TNG-{year}-{nextNumber:D4}";
        }

        public async Task<Document> CreateDocumentAsync(DocumentCreateViewModel model, int createdByUserId)
        {
            var trackingNumber = await GenerateTrackingNumberAsync();

            // Resolve document type config for step count
            var typeConfig = await _db.DocumentTypeConfigs
                .FirstOrDefaultAsync(t => t.TypeName == model.DocumentType);
            int totalSteps = typeConfig?.TotalSteps ?? 5;

            var document = new Document
            {
                TrackingNumber = trackingNumber,
                Title = model.Title,
                DocumentType = model.DocumentType,
                TypeId = typeConfig?.Id,
                OriginatingDepartment = model.OriginatingDepartment,
                SubmittedBy = model.SubmittedBy,
                ContactNumber = model.ContactNumber,
                EmailAddress = model.EmailAddress,
                CurrentStatus = DocumentStatus.Submitted,
                CurrentOfficeName = "Receiving Section",
                CurrentStepIndex = 1,
                TotalSteps = totalSteps,
                ARTAProcessingDays = model.ARTAProcessingDays,
                DateFiled = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                CreatedByUserId = createdByUserId
            };

            // Set department FK if available
            if (!string.IsNullOrEmpty(model.OriginatingDepartment))
            {
                var dept = await _db.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentName == model.OriginatingDepartment);
                if (dept != null)
                    document.DepartmentId = dept.Id;
            }

            // Calculate ARTA escalation deadline
            document.EscalationDeadline = CalculateWorkingDaysDeadline(
                document.DateFiled, document.ARTAProcessingDays);

            _db.Documents.Add(document);
            await _db.SaveChangesAsync();

            // Create Routing Slip (Table 16)
            var routingSlip = new RoutingSlip
            {
                DocumentId = document.Id,
                TrackingNumber = trackingNumber,
                ReceivedByUserId = createdByUserId,
                DateReceived = DateTime.UtcNow,
                SenderName = model.SubmittedBy,
                ActionInstruction = model.ActionInstruction,
                TargetDepartmentId = model.TargetDepartmentId,
                SlipStatus = "Active",
                CreatedAt = DateTime.UtcNow
            };
            _db.RoutingSlips.Add(routingSlip);

            // Create Document Metadata if applicable (Table 7)
            if (!string.IsNullOrEmpty(model.ConferenceName) ||
                !string.IsNullOrEmpty(model.SourceLink) ||
                !string.IsNullOrEmpty(model.Province) ||
                !string.IsNullOrEmpty(model.ReportNumber) ||
                !string.IsNullOrEmpty(model.CategoryFlags))
            {
                var metadata = new DocumentMetadata
                {
                    DocumentId = document.Id,
                    ConferenceName = model.ConferenceName,
                    SourceLink = model.SourceLink,
                    Province = model.Province,
                    ReportNumber = model.ReportNumber,
                    CategoryFlags = model.CategoryFlags
                };
                _db.DocumentMetadatas.Add(metadata);
            }

            await _db.SaveChangesAsync();

            return document;
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _db.Documents
                .Include(d => d.CreatedBy)
                .Include(d => d.Transitions).ThenInclude(t => t.PerformedBy)
                .Include(d => d.Comments).ThenInclude(c => c.User)
                .Include(d => d.Signatures).ThenInclude(s => s.SignedBy)
                .Include(d => d.Attachments)
                .Include(d => d.Metadata)
                .Include(d => d.RoutingSlip).ThenInclude(rs => rs!.TargetDepartment)
                .Include(d => d.RoutingSlip).ThenInclude(rs => rs!.ReceivedBy)
                .Include(d => d.RoutingSlip).ThenInclude(rs => rs!.NotedBy)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Document?> GetByTrackingNumberAsync(string trackingNumber)
        {
            return await _db.Documents
                .Include(d => d.Transitions).ThenInclude(t => t.PerformedBy)
                .Include(d => d.Comments).ThenInclude(c => c.User)
                .Include(d => d.Signatures).ThenInclude(s => s.SignedBy)
                .Include(d => d.Attachments)
                .Include(d => d.Metadata)
                .Include(d => d.RoutingSlip)
                .FirstOrDefaultAsync(d => d.TrackingNumber == trackingNumber);
        }

        public async Task<DocumentListViewModel> GetDocumentListAsync(
            string? search, string? docType, string? department, string? status,
            int page = 1, int pageSize = 12)
        {
            var query = _db.Documents.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.TrackingNumber.Contains(search) ||
                    d.Title.Contains(search) ||
                    d.SubmittedBy.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(docType) && docType != "All Types")
                query = query.Where(d => d.DocumentType == docType);

            if (!string.IsNullOrWhiteSpace(department) && department != "All Departments")
                query = query.Where(d => d.OriginatingDepartment == department);

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                if (Enum.TryParse<DocumentStatus>(status, out var statusEnum))
                    query = query.Where(d => d.CurrentStatus == statusEnum);
            }

            var totalCount = await query.CountAsync();

            var documents = await query
                .OrderByDescending(d => d.DateFiled)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DocumentListItem
                {
                    Id = d.Id,
                    TrackingNumber = d.TrackingNumber,
                    DocumentType = d.DocumentType,
                    SubmittedBy = d.SubmittedBy,
                    Department = d.OriginatingDepartment,
                    DateFiled = d.DateFiled,
                    CurrentStep = d.CurrentStepIndex,
                    TotalSteps = d.TotalSteps,
                    Status = d.CurrentStatus
                })
                .ToListAsync();

            return new DocumentListViewModel
            {
                Documents = documents,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                SearchQuery = search,
                FilterDocType = docType,
                FilterDepartment = department,
                FilterStatus = status
            };
        }

        public async Task<DocumentDetailViewModel> GetDocumentDetailAsync(int id, UserRole userRole)
        {
            var doc = await GetByIdAsync(id);
            if (doc == null) return new DocumentDetailViewModel();

            return new DocumentDetailViewModel
            {
                Id = doc.Id,
                TrackingNumber = doc.TrackingNumber,
                Title = doc.Title,
                DocumentType = doc.DocumentType,
                OriginatingDepartment = doc.OriginatingDepartment,
                SubmittedBy = doc.SubmittedBy,
                ContactNumber = doc.ContactNumber,
                CurrentStatus = doc.CurrentStatus,
                CurrentOfficeName = doc.CurrentOfficeName,
                CurrentStepIndex = doc.CurrentStepIndex,
                TotalSteps = doc.TotalSteps,
                DateFiled = doc.DateFiled,
                DateCompleted = doc.DateCompleted,
                LastUpdated = doc.LastUpdated,
                ARTAProcessingDays = doc.ARTAProcessingDays,
                IsEscalated = doc.IsEscalated,
                IsLocked = doc.IsLocked,
                AttachmentPath = doc.AttachmentPath,
                QRCodePath = doc.QRCodePath,
                Transitions = doc.Transitions.OrderBy(t => t.TransitionDate).ToList(),
                Comments = doc.Comments.OrderByDescending(c => c.PostedAt).ToList(),
                Signatures = doc.Signatures.OrderByDescending(s => s.SignedAt).ToList(),
                Attachments = doc.Attachments.OrderByDescending(a => a.UploadedAt).ToList(),
                RoutingSlip = doc.RoutingSlip,
                Metadata = doc.Metadata,

                // RBAC permissions per specification
                CanApprove = userRole == UserRole.Mayor,
                CanReview = userRole == UserRole.ExecutiveAdmin,
                CanForward = userRole == UserRole.RecordsOfficer || userRole == UserRole.ExecutiveAdmin,
                CanReturn = userRole == UserRole.Mayor || userRole == UserRole.ExecutiveAdmin,
                CanSign = userRole == UserRole.Mayor,  // Only Mayor can sign
                CanComment = userRole == UserRole.Mayor || userRole == UserRole.ExecutiveAdmin,
                CanEndorse = userRole == UserRole.ExecutiveAdmin,
                UserRole = userRole
            };
        }

        public async Task UpdateDocumentAsync(Document document)
        {
            document.LastUpdated = DateTime.UtcNow;
            _db.Documents.Update(document);
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _db.Documents.CountAsync();
        }

        public async Task<int> GetCountByStatusAsync(params DocumentStatus[] statuses)
        {
            return await _db.Documents.CountAsync(d => statuses.Contains(d.CurrentStatus));
        }

        public async Task<int> GetReceivedTodayCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _db.Documents.CountAsync(d => d.DateFiled >= today);
        }

        public async Task<Dictionary<string, int>> GetStatusDistributionAsync()
        {
            return await _db.Documents
                .GroupBy(d => d.CurrentStatus)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetDepartmentActivityAsync()
        {
            return await _db.Documents
                .GroupBy(d => d.OriginatingDepartment)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<List<string>> GetDocumentTypesAsync()
        {
            return await _db.DocumentTypeConfigs
                .Select(t => t.TypeName)
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<List<DepartmentDropdownItem>> GetDepartmentOptionsAsync()
        {
            return await _db.Departments
                .Select(d => new DepartmentDropdownItem
                {
                    Id = d.Id,
                    Name = d.DepartmentName,
                    Code = d.DepartmentCode
                })
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Calculate a deadline date by adding working days (excluding weekends).
        /// </summary>
        private DateTime CalculateWorkingDaysDeadline(DateTime startDate, int workingDays)
        {
            var date = startDate;
            int addedDays = 0;
            while (addedDays < workingDays)
            {
                date = date.AddDays(1);
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    addedDays++;
            }
            return date;
        }
    }
}
