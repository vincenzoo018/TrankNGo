using Microsoft.EntityFrameworkCore;
using TrackNGo.Data;
using TrackNGo.Models;
using TrackNGo.ViewModels;

namespace TrackNGo.Services
{
    public interface IReportService
    {
        Task<ReportGenerateViewModel> GenerateDocumentSummaryAsync(DateTime dateFrom, DateTime dateTo, int userId);
        Task<ReportGenerateViewModel> GenerateDepartmentPerformanceAsync(DateTime dateFrom, DateTime dateTo, int userId);
        Task<ReportGenerateViewModel> GenerateWorkflowReportAsync(DateTime dateFrom, DateTime dateTo, int userId);
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditTrailService _audit;

        public ReportService(ApplicationDbContext db, IAuditTrailService audit)
        {
            _db = db;
            _audit = audit;
        }

        public async Task<ReportGenerateViewModel> GenerateDocumentSummaryAsync(DateTime dateFrom, DateTime dateTo, int userId)
        {
            var docs = await _db.Documents
                .Where(d => d.DateFiled >= dateFrom && d.DateFiled <= dateTo)
                .ToListAsync();

            var data = docs
                .GroupBy(d => d.CurrentStatus.ToString())
                .Select(g => new ReportDataRow
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Add type breakdown
            var byType = docs
                .GroupBy(d => d.DocumentType)
                .Select(g => new ReportDataRow
                {
                    Label = $"Type: {g.Key}",
                    Count = g.Count()
                })
                .ToList();
            data.AddRange(byType);

            // Add department breakdown
            var byDept = docs
                .GroupBy(d => d.OriginatingDepartment)
                .Select(g => new ReportDataRow
                {
                    Label = $"Dept: {g.Key}",
                    Count = g.Count()
                })
                .ToList();
            data.AddRange(byDept);

            // Log the report
            await LogReport(userId, "Document Summary", dateFrom, dateTo);

            return new ReportGenerateViewModel
            {
                ReportType = "Document Summary",
                DateFrom = dateFrom,
                DateTo = dateTo,
                ReportData = data,
                IsGenerated = true
            };
        }

        public async Task<ReportGenerateViewModel> GenerateDepartmentPerformanceAsync(DateTime dateFrom, DateTime dateTo, int userId)
        {
            var docs = await _db.Documents
                .Where(d => d.DateFiled >= dateFrom && d.DateFiled <= dateTo && d.DateCompleted.HasValue)
                .ToListAsync();

            var data = docs
                .GroupBy(d => d.OriginatingDepartment)
                .Select(g => new ReportDataRow
                {
                    Label = g.Key,
                    Count = g.Count(),
                    AverageDays = g.Average(d => (d.DateCompleted!.Value - d.DateFiled).TotalDays),
                    Extra = g.Any(d => d.IsEscalated) ? $"{g.Count(d => d.IsEscalated)} escalated" : "None escalated"
                })
                .OrderByDescending(r => r.Count)
                .ToList();

            await LogReport(userId, "Department Performance", dateFrom, dateTo);

            return new ReportGenerateViewModel
            {
                ReportType = "Department Performance",
                DateFrom = dateFrom,
                DateTo = dateTo,
                ReportData = data,
                IsGenerated = true
            };
        }

        public async Task<ReportGenerateViewModel> GenerateWorkflowReportAsync(DateTime dateFrom, DateTime dateTo, int userId)
        {
            var transitions = await _db.WorkflowTransitions
                .Include(t => t.Document)
                .Where(t => t.TransitionDate >= dateFrom && t.TransitionDate <= dateTo)
                .ToListAsync();

            var data = transitions
                .GroupBy(t => t.ActionTaken)
                .Select(g => new ReportDataRow
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Extra = $"Offices: {string.Join(", ", g.Select(t => t.ToOffice).Distinct().Take(3))}"
                })
                .OrderByDescending(r => r.Count)
                .ToList();

            await LogReport(userId, "Workflow Report", dateFrom, dateTo);

            return new ReportGenerateViewModel
            {
                ReportType = "Workflow Report",
                DateFrom = dateFrom,
                DateTo = dateTo,
                ReportData = data,
                IsGenerated = true
            };
        }

        private async Task LogReport(int userId, string reportType, DateTime dateFrom, DateTime dateTo)
        {
            var log = new ReportLog
            {
                GeneratedByUserId = userId,
                ReportType = reportType,
                DateFrom = dateFrom,
                DateTo = dateTo,
                GeneratedAt = DateTime.UtcNow
            };
            _db.ReportLogs.Add(log);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(null, userId, "ReportGenerated",
                $"Report generated: {reportType} ({dateFrom:yyyy-MM-dd} to {dateTo:yyyy-MM-dd})");
        }
    }
}
