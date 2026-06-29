using QRCoder;
using TrackNGo.Data;
using TrackNGo.Models;

namespace TrackNGo.Services
{
    public interface IQRCodeService
    {
        /// <summary>Generate a QR code for a document and save it to disk.</summary>
        Task<QRCodeRecord> GenerateQRCodeAsync(Document document, string baseUrl);

        /// <summary>Get the QR code record for a document.</summary>
        Task<QRCodeRecord?> GetByDocumentIdAsync(int documentId);

        /// <summary>Increment scan count when a QR code is scanned.</summary>
        Task IncrementScanCountAsync(string trackingNumber);
    }

    public class QRCodeService : IQRCodeService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public QRCodeService(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<QRCodeRecord> GenerateQRCodeAsync(Document document, string baseUrl)
        {
            var trackingUrl = $"{baseUrl}/Public/Track?ref={document.TrackingNumber}";

            // Generate QR code image using QRCoder
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(trackingUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(10);

            // Save to wwwroot/qrcodes/
            var qrDir = Path.Combine(_env.WebRootPath, "qrcodes");
            Directory.CreateDirectory(qrDir);

            var fileName = $"{document.TrackingNumber}.png";
            var filePath = Path.Combine(qrDir, fileName);
            await File.WriteAllBytesAsync(filePath, qrCodeBytes);

            var relativePath = $"/qrcodes/{fileName}";

            // Update document with QR path
            document.QRCodePath = relativePath;
            _db.Documents.Update(document);

            // Create QR record
            var record = new QRCodeRecord
            {
                DocumentId = document.Id,
                TrackingNumber = document.TrackingNumber,
                QRCodeImagePath = relativePath,
                TrackingUrl = trackingUrl,
                GeneratedAt = DateTime.UtcNow
            };
            _db.QRCodeRecords.Add(record);
            await _db.SaveChangesAsync();

            return record;
        }

        public async Task<QRCodeRecord?> GetByDocumentIdAsync(int documentId)
        {
            return await Task.FromResult(
                _db.QRCodeRecords.FirstOrDefault(q => q.DocumentId == documentId));
        }

        public async Task IncrementScanCountAsync(string trackingNumber)
        {
            var record = _db.QRCodeRecords.FirstOrDefault(q => q.TrackingNumber == trackingNumber);
            if (record != null)
            {
                record.ScanCount++;
                record.LastScannedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }
}
