using System.Security.Cryptography;
using System.Text;
using TrackNGo.Data;
using TrackNGo.Models;

namespace TrackNGo.Services
{
    public interface ISignatureService
    {
        /// <summary>Save a digital signature for a document.</summary>
        Task<DigitalSignature> SignDocumentAsync(int documentId, int userId, string signatureBase64,
            string actionType, string? remarks = null);

        /// <summary>Verify a signature's hash integrity.</summary>
        Task<bool> VerifySignatureAsync(int signatureId);

        /// <summary>Get all signatures for a document.</summary>
        Task<List<DigitalSignature>> GetByDocumentIdAsync(int documentId);
    }

    public class SignatureService : ISignatureService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IAuditTrailService _audit;

        public SignatureService(ApplicationDbContext db, IWebHostEnvironment env, IAuditTrailService audit)
        {
            _db = db;
            _env = env;
            _audit = audit;
        }

        public async Task<DigitalSignature> SignDocumentAsync(int documentId, int userId,
            string signatureBase64, string actionType, string? remarks = null)
        {
            // Decode base64 signature image and save to disk
            var sigDir = Path.Combine(_env.WebRootPath, "signatures");
            Directory.CreateDirectory(sigDir);

            var fileName = $"sig_{documentId}_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var filePath = Path.Combine(sigDir, fileName);

            // Remove data URL prefix if present
            var base64Data = signatureBase64;
            if (base64Data.Contains(","))
                base64Data = base64Data.Split(',')[1];

            var imageBytes = Convert.FromBase64String(base64Data);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            // Generate SHA-256 hash for verification
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(imageBytes);
            var hashString = Convert.ToBase64String(hash);

            var signature = new DigitalSignature
            {
                DocumentId = documentId,
                SignedByUserId = userId,
                SignatureImagePath = $"/signatures/{fileName}",
                SignatureHash = hashString,
                ActionType = actionType,
                Remarks = remarks,
                SignedAt = DateTime.UtcNow,
                IsVerified = true
            };

            _db.DigitalSignatures.Add(signature);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(documentId, userId, "DocumentSigned",
                $"Document digitally signed. Action: {actionType}");

            return signature;
        }

        public async Task<bool> VerifySignatureAsync(int signatureId)
        {
            var signature = await _db.DigitalSignatures.FindAsync(signatureId);
            if (signature == null) return false;

            var filePath = Path.Combine(_env.WebRootPath, signature.SignatureImagePath.TrimStart('/'));
            if (!File.Exists(filePath)) return false;

            var imageBytes = await File.ReadAllBytesAsync(filePath);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(imageBytes);
            var computedHash = Convert.ToBase64String(hash);

            return computedHash == signature.SignatureHash;
        }

        public async Task<List<DigitalSignature>> GetByDocumentIdAsync(int documentId)
        {
            return await Task.FromResult(
                _db.DigitalSignatures
                    .Where(s => s.DocumentId == documentId)
                    .OrderByDescending(s => s.SignedAt)
                    .ToList());
        }
    }
}
