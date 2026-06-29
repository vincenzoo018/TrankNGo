using Microsoft.EntityFrameworkCore;
using TrackNGo.Data;
using TrackNGo.Models;

namespace TrackNGo.Services
{
    public interface IAuditTrailService
    {
        Task LogAsync(int? documentId, int? userId, string action, string details,
            string? oldValue = null, string? newValue = null, string? ipAddress = null);
        Task<List<AuditTrailEntry>> GetByDocumentAsync(int documentId);
        Task<List<AuditTrailEntry>> GetRecentAsync(int count = 50);
        Task<List<AuditTrailEntry>> GetAllAsync(int page = 1, int pageSize = 50);
    }

    public class AuditTrailService : IAuditTrailService
    {
        private readonly ApplicationDbContext _db;

        public AuditTrailService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(int? documentId, int? userId, string action, string details,
            string? oldValue = null, string? newValue = null, string? ipAddress = null)
        {
            var entry = new AuditTrailEntry
            {
                DocumentId = documentId,
                UserId = userId,
                Action = action,
                Details = details,
                OldValue = oldValue,
                NewValue = newValue,
                IPAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
            _db.AuditTrailEntries.Add(entry);
            await _db.SaveChangesAsync();
        }

        public async Task<List<AuditTrailEntry>> GetByDocumentAsync(int documentId)
        {
            return await Task.FromResult(
                _db.AuditTrailEntries
                    .Where(a => a.DocumentId == documentId)
                    .OrderByDescending(a => a.Timestamp)
                    .ToList());
        }

        public async Task<List<AuditTrailEntry>> GetRecentAsync(int count = 50)
        {
            return await Task.FromResult(
                _db.AuditTrailEntries
                    .Include(a => a.Document)
                    .Include(a => a.User)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(count)
                    .ToList());
        }

        public async Task<List<AuditTrailEntry>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            return await Task.FromResult(
                _db.AuditTrailEntries
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());
        }
    }
}
