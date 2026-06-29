using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TrackNGo.Data;
using TrackNGo.Models;

namespace TrackNGo.Services
{
    public interface IExportService
    {
        /// <summary>Validate the secondary export password for a user.</summary>
        Task<bool> ValidateExportPasswordAsync(int userId, string exportPassword);

        /// <summary>Set/update the secondary export password for a user.</summary>
        Task SetExportPasswordAsync(int userId, string exportPassword);

        /// <summary>Export documents as CSV string.</summary>
        Task<string> ExportDocumentsCSVAsync(string? docType, string? department,
            string? status, DateTime? dateFrom, DateTime? dateTo);

        /// <summary>Log an export action.</summary>
        Task LogExportAsync(int userId, string exportType, string exportScope,
            int recordCount, string status, string? ipAddress = null);

        /// <summary>Get export audit logs.</summary>
        Task<List<ExportAuditLog>> GetExportLogsAsync(int count = 50);
    }

    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditTrailService _audit;

        public ExportService(ApplicationDbContext db, IAuditTrailService audit)
        {
            _db = db;
            _audit = audit;
        }

        public async Task<bool> ValidateExportPasswordAsync(int userId, string exportPassword)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.ExportPasswordHash)) return false;

            using var sha256 = SHA256.Create();
            var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(exportPassword)));
            bool isValid = hash == user.ExportPasswordHash;
            
            if (!isValid)
            {
                await _audit.LogAsync(null, userId, "ExportFailed", "Failed attempt to use secondary export password.");
            }
            
            return isValid;
        }

        public async Task SetExportPasswordAsync(int userId, string exportPassword)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return;

            using var sha256 = SHA256.Create();
            user.ExportPasswordHash = Convert.ToBase64String(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(exportPassword)));
            await _db.SaveChangesAsync();
        }

        public async Task<string> ExportDocumentsCSVAsync(string? docType, string? department,
            string? status, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _db.Documents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(docType) && docType != "All")
                query = query.Where(d => d.DocumentType == docType);
            if (!string.IsNullOrWhiteSpace(department) && department != "All")
                query = query.Where(d => d.OriginatingDepartment == department);
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                if (Enum.TryParse<DocumentStatus>(status, out var s))
                    query = query.Where(d => d.CurrentStatus == s);
            }
            if (dateFrom.HasValue)
                query = query.Where(d => d.DateFiled >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(d => d.DateFiled <= dateTo.Value);

            var documents = await query.OrderByDescending(d => d.DateFiled).ToListAsync();

            // Build CSV
            var sb = new StringBuilder();
            sb.AppendLine("Tracking Number,Document Type,Title,Submitted By,Department,Date Filed,Status,Current Office,ARTA Days,Is Escalated");

            foreach (var d in documents)
            {
                sb.AppendLine($"\"{d.TrackingNumber}\",\"{d.DocumentType}\",\"{d.Title}\",\"{d.SubmittedBy}\"," +
                    $"\"{d.OriginatingDepartment}\",\"{d.DateFiled:yyyy-MM-dd}\",\"{d.CurrentStatus}\"," +
                    $"\"{d.CurrentOfficeName}\",{d.ARTAProcessingDays},{d.IsEscalated}");
            }

            return sb.ToString();
        }

        public async Task LogExportAsync(int userId, string exportType, string exportScope,
            int recordCount, string status, string? ipAddress = null)
        {
            var log = new ExportAuditLog
            {
                UserId = userId,
                ExportType = exportType,
                ExportScope = exportScope,
                RecordCount = recordCount,
                ExportedAt = DateTime.UtcNow,
                IPAddress = ipAddress
            };
            
            // Add ExportStatus if the model supported it, but it seems we didn't add it to ExportAuditLog yet.
            // For now we'll append it to the scope.
            log.ExportScope = $"{exportScope} (Status: {status})";
            
            _db.ExportAuditLogs.Add(log);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(null, userId, "DataExported",
                $"Data exported as {exportType}. Scope: {exportScope}. Records: {recordCount}. Status: {status}");
        }

        public async Task<List<ExportAuditLog>> GetExportLogsAsync(int count = 50)
        {
            return await _db.ExportAuditLogs
                .Include(e => e.User)
                .OrderByDescending(e => e.ExportedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
