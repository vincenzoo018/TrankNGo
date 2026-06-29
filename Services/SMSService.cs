using TrackNGo.Data;
using TrackNGo.Models;

namespace TrackNGo.Services
{
    public interface ISMSService
    {
        /// <summary>Send an SMS notification triggered by a document event.</summary>
        Task SendNotificationAsync(int documentId, string recipientNumber, string recipientName,
            string triggerEvent, string messageContent);

        /// <summary>Send a templated SMS based on trigger event type.</summary>
        Task SendTemplatedNotificationAsync(Document document, string triggerEvent, string? additionalInfo = null);

        /// <summary>Get all notifications for a document.</summary>
        Task<List<SMSNotification>> GetByDocumentIdAsync(int documentId);

        /// <summary>Get recent notifications across all documents.</summary>
        Task<List<SMSNotification>> GetRecentAsync(int count = 50);
    }

    public class SMSService : ISMSService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SMSService> _logger;

        public SMSService(ApplicationDbContext db, ILogger<SMSService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// SMS message templates per trigger event.
        /// Placeholders: {ref}, {status}, {office}, {remarks}, {days}
        /// </summary>
        private static readonly Dictionary<string, string> Templates = new()
        {
            { "DocumentReceived", "TrackNGo Mati: Your document {ref} has been received and is now being processed at the Office of the Mayor." },
            { "StatusChanged", "TrackNGo Mati: Update – Your document {ref} is now {status} at {office}." },
            { "UnderReview", "TrackNGo Mati: Your document {ref} is now under review at {office}." },
            { "ApprovalPending", "TrackNGo Mati: Document {ref} requires the Mayor's approval." },
            { "Approved", "TrackNGo Mati: Your document {ref} has been approved by the Mayor." },
            { "Returned", "TrackNGo Mati: Your document {ref} has been returned for revision. Reason: {remarks}" },
            { "Rejected", "TrackNGo Mati: Your document {ref} has been rejected. Reason: {remarks}" },
            { "Released", "TrackNGo Mati: Your document {ref} is ready for pickup/release at the Office of the Mayor." },
            { "Escalated", "TrackNGo Mati: ARTA Alert – Document {ref} has exceeded the {days}-day processing period and has been escalated." },
            { "FeedbackRequest", "TrackNGo Mati: Please rate your experience with document {ref}. Thank you!" }
        };

        public async Task SendNotificationAsync(int documentId, string recipientNumber, string recipientName,
            string triggerEvent, string messageContent)
        {
            var notification = new SMSNotification
            {
                DocumentId = documentId,
                RecipientNumber = recipientNumber,
                RecipientName = recipientName,
                MessageContent = messageContent,
                TriggerEvent = triggerEvent,
                Status = SMSStatus.Queued,
                QueuedAt = DateTime.UtcNow
            };

            _db.SMSNotifications.Add(notification);
            await _db.SaveChangesAsync();

            // Simulate SMS sending (in production, integrate with Semaphore/Twilio gateway)
            try
            {
                // TODO: Integrate with actual SMS gateway
                // await _smsGateway.SendAsync(recipientNumber, messageContent);
                notification.Status = SMSStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
                notification.GatewayResponse = "Simulated: Message sent successfully";
                _logger.LogInformation("SMS sent to {Number}: {Message}", recipientNumber, messageContent);
            }
            catch (Exception ex)
            {
                notification.Status = SMSStatus.Failed;
                notification.GatewayResponse = $"Error: {ex.Message}";
                _logger.LogError(ex, "Failed to send SMS to {Number}", recipientNumber);
            }

            await _db.SaveChangesAsync();
        }

        public async Task SendTemplatedNotificationAsync(Document document, string triggerEvent,
            string? additionalInfo = null)
        {
            if (string.IsNullOrWhiteSpace(document.ContactNumber)) return;

            if (!Templates.TryGetValue(triggerEvent, out var template)) return;

            var message = template
                .Replace("{ref}", document.TrackingNumber)
                .Replace("{status}", document.CurrentStatus.ToString())
                .Replace("{office}", document.CurrentOfficeName)
                .Replace("{remarks}", additionalInfo ?? "No additional details")
                .Replace("{days}", document.ARTAProcessingDays.ToString());

            await SendNotificationAsync(
                document.Id,
                document.ContactNumber,
                document.SubmittedBy,
                triggerEvent,
                message);
        }

        public async Task<List<SMSNotification>> GetByDocumentIdAsync(int documentId)
        {
            return await Task.FromResult(
                _db.SMSNotifications
                    .Where(n => n.DocumentId == documentId)
                    .OrderByDescending(n => n.QueuedAt)
                    .ToList());
        }

        public async Task<List<SMSNotification>> GetRecentAsync(int count = 50)
        {
            return await Task.FromResult(
                _db.SMSNotifications
                    .OrderByDescending(n => n.QueuedAt)
                    .Take(count)
                    .ToList());
        }
    }
}
