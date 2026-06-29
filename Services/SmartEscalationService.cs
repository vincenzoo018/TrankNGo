using Microsoft.EntityFrameworkCore;
using TrackNGo.Data;
using TrackNGo.Models;
using TrackNGo.ViewModels;

namespace TrackNGo.Services
{
    /// <summary>
    /// CAPSTONE ELEMENT: EODB-Compliant Smart Escalation Routing Algorithm.
    /// Monitors document processing against ARTA deadlines (3, 7, 20 working days)
    /// and automatically escalates overdue documents per RA 11032.
    /// Implements a 3-tier escalation system (Warning, Critical, Overdue).
    /// </summary>
    public interface ISmartEscalationService
    {
        /// <summary>Check all active documents for ARTA compliance violations.</summary>
        Task<int> CheckAndEscalateAsync();

        /// <summary>Calculate elapsed working days for a document.</summary>
        int CalculateElapsedWorkingDays(DateTime startDate, DateTime endDate);

        /// <summary>Get all currently escalated documents.</summary>
        Task<List<EscalationAlertItem>> GetEscalatedDocumentsAsync();

        /// <summary>Get documents nearing their ARTA deadline (warning threshold: 75%).</summary>
        Task<List<ARTAWarningItem>> GetWarningDocumentsAsync();

        /// <summary>Resolve an escalation (Oversight Officer intervenes).</summary>
        Task ResolveEscalationAsync(int documentId, int resolvedByUserId, string resolutionNotes);

        /// <summary>Get the overall ARTA compliance rate.</summary>
        Task<double> GetComplianceRateAsync();
    }

    public class SmartEscalationService : ISmartEscalationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditTrailService _audit;
        private readonly ILogger<SmartEscalationService> _logger;
        private readonly ISMSService _smsService;

        public SmartEscalationService(ApplicationDbContext db, IAuditTrailService audit,
            ILogger<SmartEscalationService> logger, ISMSService smsService)
        {
            _db = db;
            _audit = audit;
            _logger = logger;
            _smsService = smsService;
        }

        /// <summary>
        /// CORE ALGORITHM: Check all active documents against ARTA processing deadlines.
        /// If a document reaches a threshold, log it and send SMS notifications.
        /// Returns the count of newly escalated documents.
        /// </summary>
        public async Task<int> CheckAndEscalateAsync()
        {
            var now = DateTime.UtcNow;
            int escalatedCount = 0;

            // Get all active documents
            var activeDocuments = await _db.Documents
                .Include(d => d.DepartmentEntity)
                .Where(d => d.CurrentStatus != DocumentStatus.Completed
                         && d.CurrentStatus != DocumentStatus.Rejected
                         && !d.IsLocked)
                .ToListAsync();

            // Get oversight officer for notifications
            var oversightUser = await _db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.OversightOfficer);

            foreach (var document in activeDocuments)
            {
                var elapsedDays = CalculateElapsedWorkingDays(document.DateFiled, now);
                var previousStatus = document.CurrentStatus;
                var previousOffice = document.CurrentOfficeName;
                var artaDays = document.ARTAProcessingDays;

                string? escalationLevel = null;

                // 3-tier escalation logic
                if (elapsedDays >= artaDays)
                {
                    escalationLevel = "Overdue";
                    if (!document.IsEscalated)
                    {
                        document.IsEscalated = true;
                        document.CurrentStatus = DocumentStatus.Escalated;
                        document.IsLocked = true; // Lock for Mayor intervention
                        document.CurrentOfficeName = "Office of the Mayor";
                        document.CurrentStepIndex++;
                    }
                }
                else if (elapsedDays >= artaDays - 1)
                {
                    escalationLevel = "Critical";
                }
                else if (elapsedDays >= (int)Math.Ceiling(artaDays * 0.75))
                {
                    escalationLevel = "Warning";
                }

                if (escalationLevel != null)
                {
                    // Check if this specific level for this document has already been logged today
                    var alreadyLogged = await _db.EscalationLogs.AnyAsync(e =>
                        e.DocumentId == document.Id &&
                        e.EscalationLevel == escalationLevel &&
                        e.EscalatedAt.Date == now.Date);

                    if (!alreadyLogged)
                    {
                        // 1. Record the violation in EscalationLog
                        var escalationLog = new EscalationLog
                        {
                            DocumentId = document.Id,
                            ViolatingOffice = previousOffice,
                            ARTAThreshold = artaDays,
                            ARTAPeriodDays = artaDays, // legacy field
                            ActualElapsedDays = elapsedDays,
                            EscalationLevel = escalationLevel,
                            EscalationReason = $"ARTA {escalationLevel}: Document at {elapsedDays}/{artaDays} working days. " +
                                $"Current status: {previousStatus} at {previousOffice}.",
                            EscalatedAt = now,
                            NotifiedUserId = oversightUser?.Id,
                            NotificationSent = false
                        };
                        _db.EscalationLogs.Add(escalationLog);

                        // 2. Notifications based on level
                        if (escalationLevel == "Warning" && oversightUser?.MobileNumber != null)
                        {
                            await _smsService.SendNotificationAsync(document.Id, oversightUser.MobileNumber, oversightUser.FullName, "ARTA_Warning",
                                $"TrackNGo Alert: Document {document.TrackingNumber} is nearing ARTA deadline ({elapsedDays}/{artaDays} days) at {previousOffice}.");
                            escalationLog.NotificationSent = true;
                        }
                        else if (escalationLevel == "Critical")
                        {
                            // Notify Oversight + Dept Head
                            if (oversightUser?.MobileNumber != null)
                            {
                                await _smsService.SendNotificationAsync(document.Id, oversightUser.MobileNumber, oversightUser.FullName, "ARTA_Critical",
                                    $"TrackNGo CRITICAL: Document {document.TrackingNumber} at {previousOffice} has 1 day left ({elapsedDays}/{artaDays} days).");
                            }
                            if (document.DepartmentEntity?.HeadUserId != null)
                            {
                                var head = await _db.Users.FindAsync(document.DepartmentEntity.HeadUserId);
                                if (head?.MobileNumber != null)
                                {
                                     await _smsService.SendNotificationAsync(document.Id, head.MobileNumber, head.FullName, "ARTA_Critical_Dept",
                                        $"TrackNGo CRITICAL: Your dept has a document ({document.TrackingNumber}) nearing its ARTA deadline ({elapsedDays}/{artaDays} days).");
                                }
                            }
                            escalationLog.NotificationSent = true;
                        }
                        else if (escalationLevel == "Overdue")
                        {
                            // Notify Oversight + Mayor
                            if (oversightUser?.MobileNumber != null)
                            {
                                await _smsService.SendNotificationAsync(document.Id, oversightUser.MobileNumber, oversightUser.FullName, "ARTA_Overdue",
                                    $"TrackNGo OVERDUE: Document {document.TrackingNumber} missed deadline ({elapsedDays}/{artaDays} days). Escalated to Mayor.");
                            }
                            var mayor = await _db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Mayor);
                            if (mayor?.MobileNumber != null)
                            {
                                await _smsService.SendNotificationAsync(document.Id, mayor.MobileNumber, mayor.FullName, "ARTA_Overdue_Mayor",
                                    $"TrackNGo ALERT: Document {document.TrackingNumber} exceeded ARTA {artaDays}-day limit and requires your intervention.");
                            }
                            
                             // Record workflow transition
                            var transition = new WorkflowTransition
                            {
                                DocumentId = document.Id,
                                FromStatus = previousStatus,
                                ToStatus = DocumentStatus.Escalated,
                                FromOffice = previousOffice,
                                ToOffice = "Office of the Mayor",
                                ActionTaken = "Auto-Escalated (ARTA Violation)",
                                Remarks = $"Document automatically escalated after exceeding {document.ARTAProcessingDays}-day " +
                                    $"ARTA processing period. Elapsed: {elapsedDays} working days.",
                                PerformedByUserId = 1, // System action (admin user)
                                TransitionDate = now,
                                StepNumber = document.CurrentStepIndex
                            };
                            _db.WorkflowTransitions.Add(transition);

                             // Log to audit trail
                            await _audit.LogAsync(document.Id, null, "Escalated",
                                $"ARTA VIOLATION: Document {document.TrackingNumber} auto-escalated. " +
                                $"Period: {document.ARTAProcessingDays} days, Elapsed: {elapsedDays} days. " +
                                $"Violating office: {previousOffice}. Document locked and forwarded to Mayor's Office.",
                                previousStatus.ToString(), DocumentStatus.Escalated.ToString());
                                
                            escalatedCount++;
                        }

                        _logger.LogWarning("ARTA Escalation ({Level}): Document {TrackingNumber} at {Days}/{Limit} days. Violating office: {Office}",
                            escalationLevel, document.TrackingNumber, elapsedDays, document.ARTAProcessingDays, previousOffice);
                    }
                }
            }

            await _db.SaveChangesAsync();
            return escalatedCount;
        }

        /// <summary>
        /// Calculate the number of working days between two dates.
        /// Excludes weekends (Saturday, Sunday).
        /// </summary>
        public int CalculateElapsedWorkingDays(DateTime startDate, DateTime endDate)
        {
            int workingDays = 0;
            var current = startDate.Date.AddDays(1); // Start counting from next day

            while (current <= endDate.Date)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                    workingDays++;
                current = current.AddDays(1);
            }

            return workingDays;
        }

        public async Task<List<EscalationAlertItem>> GetEscalatedDocumentsAsync()
        {
            return await _db.EscalationLogs
                .Include(e => e.Document)
                .Where(e => !e.Resolved)
                .OrderByDescending(e => e.EscalatedAt)
                .Select(e => new EscalationAlertItem
                {
                    DocumentId = e.DocumentId,
                    TrackingNumber = e.Document!.TrackingNumber,
                    Title = e.Document.Title,
                    ViolatingOffice = e.ViolatingOffice,
                    ARTAPeriod = e.ARTAThreshold,
                    DaysOverdue = e.ActualElapsedDays - e.ARTAThreshold,
                    EscalationLevel = e.EscalationLevel,
                    EscalatedAt = e.EscalatedAt
                })
                .ToListAsync();
        }

        public async Task<List<ARTAWarningItem>> GetWarningDocumentsAsync()
        {
            var now = DateTime.UtcNow;

            var activeDocuments = await _db.Documents
                .Where(d => d.CurrentStatus != DocumentStatus.Completed
                         && d.CurrentStatus != DocumentStatus.Rejected
                         && d.CurrentStatus != DocumentStatus.Escalated
                         && !d.IsLocked)
                .ToListAsync();

            var warnings = new List<ARTAWarningItem>();
            foreach (var doc in activeDocuments)
            {
                var elapsed = CalculateElapsedWorkingDays(doc.DateFiled, now);
                var percentUsed = (double)elapsed / doc.ARTAProcessingDays * 100;

                // Warning threshold: 75% of ARTA period used
                if (percentUsed >= 75 && percentUsed < 100)
                {
                    warnings.Add(new ARTAWarningItem
                    {
                        DocumentId = doc.Id,
                        TrackingNumber = doc.TrackingNumber,
                        Title = doc.Title,
                        CurrentOffice = doc.CurrentOfficeName,
                        DaysElapsed = elapsed,
                        ARTAPeriod = doc.ARTAProcessingDays,
                        PercentageUsed = Math.Round(percentUsed, 1)
                    });
                }
            }

            return warnings.OrderByDescending(w => w.PercentageUsed).ToList();
        }

        public async Task ResolveEscalationAsync(int documentId, int resolvedByUserId, string resolutionNotes)
        {
            var escalations = await _db.EscalationLogs
                .Where(e => e.DocumentId == documentId && !e.Resolved)
                .ToListAsync();

            foreach (var escalation in escalations)
            {
                escalation.Resolved = true;
                escalation.ResolvedAt = DateTime.UtcNow;
                escalation.ResolvedByUserId = resolvedByUserId;
                escalation.ResolutionNotes = resolutionNotes;
            }
            
            await _db.SaveChangesAsync();
            
            await _audit.LogAsync(documentId, resolvedByUserId, "EscalationResolved", $"Oversight officer resolved escalation. Notes: {resolutionNotes}");
        }

        public async Task<double> GetComplianceRateAsync()
        {
            var totalCompleted = await _db.Documents
                .CountAsync(d => d.CurrentStatus == DocumentStatus.Completed);

            if (totalCompleted == 0) return 100.0;

            var completedOnTime = await _db.Documents
                .CountAsync(d => d.CurrentStatus == DocumentStatus.Completed && !d.IsEscalated);

            return Math.Round((double)completedOnTime / totalCompleted * 100, 1);
        }
    }

    /// <summary>
    /// Background hosted service that periodically runs the Smart Escalation algorithm.
    /// Checks ARTA compliance every hour (configurable).
    /// </summary>
    public class SmartEscalationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SmartEscalationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public SmartEscalationBackgroundService(IServiceScopeFactory scopeFactory,
            ILogger<SmartEscalationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Smart Escalation Background Service started. Check interval: {Interval}",
                _checkInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var escalationService = scope.ServiceProvider.GetRequiredService<ISmartEscalationService>();

                    var count = await escalationService.CheckAndEscalateAsync();
                    if (count > 0)
                    {
                        _logger.LogWarning("Smart Escalation: {Count} document(s) escalated for ARTA violations", count);
                    }
                    else
                    {
                        _logger.LogInformation("Smart Escalation check completed. No violations found.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during Smart Escalation check");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
