using Microsoft.EntityFrameworkCore;
using TrackNGo.Models;

namespace TrackNGo.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for TrackNGo Mati.
    /// Manages all database entities and relationships.
    /// Aligned to the 17-table target database specification.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ── DbSets (all 17 tables) ──
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DocumentTypeConfig> DocumentTypeConfigs { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentAttachment> DocumentAttachments { get; set; }
        public DbSet<DocumentMetadata> DocumentMetadatas { get; set; }
        public DbSet<WorkflowStep> WorkflowSteps { get; set; }
        public DbSet<WorkflowTransition> WorkflowTransitions { get; set; }
        public DbSet<DocumentComment> DocumentComments { get; set; }
        public DbSet<DigitalSignature> DigitalSignatures { get; set; }
        public DbSet<AuditTrailEntry> AuditTrailEntries { get; set; }
        public DbSet<QRCodeRecord> QRCodeRecords { get; set; }
        public DbSet<SMSNotification> SMSNotifications { get; set; }
        public DbSet<ReportLog> ReportLogs { get; set; }
        public DbSet<RoutingSlip> RoutingSlips { get; set; }
        public DbSet<EscalationLog> EscalationLogs { get; set; }
        public DbSet<ExportAuditLog> ExportAuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ══════════════════════════════════════════════════
            // INDEXES
            // ══════════════════════════════════════════════════

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.TrackingNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.CurrentStatus);

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.DateFiled);

            modelBuilder.Entity<QRCodeRecord>()
                .HasIndex(q => q.TrackingNumber)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.DepartmentCode)
                .IsUnique();

            modelBuilder.Entity<RoutingSlip>()
                .HasIndex(r => r.TrackingNumber)
                .IsUnique();

            // ══════════════════════════════════════════════════
            // RELATIONSHIP CONFIGURATIONS
            // ══════════════════════════════════════════════════

            // ── Department → HeadUser ──
            modelBuilder.Entity<Department>()
                .HasOne(d => d.HeadUser)
                .WithMany()
                .HasForeignKey(d => d.HeadUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── User → Department ──
            modelBuilder.Entity<User>()
                .HasOne(u => u.DepartmentEntity)
                .WithMany()
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Document → CreatedBy (User) ──
            modelBuilder.Entity<Document>()
                .HasOne(d => d.CreatedBy)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Document → SubmittedByUser ──
            modelBuilder.Entity<Document>()
                .HasOne(d => d.SubmittedByUser)
                .WithMany()
                .HasForeignKey(d => d.SubmittedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Document → DocumentTypeConfig ──
            modelBuilder.Entity<Document>()
                .HasOne(d => d.DocumentTypeConfig)
                .WithMany()
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Document → Department ──
            modelBuilder.Entity<Document>()
                .HasOne(d => d.DepartmentEntity)
                .WithMany()
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── DocumentAttachment → Document ──
            modelBuilder.Entity<DocumentAttachment>()
                .HasOne(da => da.Document)
                .WithMany(d => d.Attachments)
                .HasForeignKey(da => da.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── DocumentMetadata → Document (one-to-one) ──
            modelBuilder.Entity<DocumentMetadata>()
                .HasOne(dm => dm.Document)
                .WithOne(d => d.Metadata)
                .HasForeignKey<DocumentMetadata>(dm => dm.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── RoutingSlip → Document (one-to-one) ──
            modelBuilder.Entity<RoutingSlip>()
                .HasOne(rs => rs.Document)
                .WithOne(d => d.RoutingSlip)
                .HasForeignKey<RoutingSlip>(rs => rs.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── RoutingSlip → ReceivedBy ──
            modelBuilder.Entity<RoutingSlip>()
                .HasOne(rs => rs.ReceivedBy)
                .WithMany()
                .HasForeignKey(rs => rs.ReceivedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── RoutingSlip → NotedBy ──
            modelBuilder.Entity<RoutingSlip>()
                .HasOne(rs => rs.NotedBy)
                .WithMany()
                .HasForeignKey(rs => rs.NotedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── RoutingSlip → TargetDepartment ──
            modelBuilder.Entity<RoutingSlip>()
                .HasOne(rs => rs.TargetDepartment)
                .WithMany()
                .HasForeignKey(rs => rs.TargetDepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── WorkflowStep → DocumentTypeConfig ──
            modelBuilder.Entity<WorkflowStep>()
                .HasOne(ws => ws.DocumentType)
                .WithMany(dt => dt.WorkflowSteps)
                .HasForeignKey(ws => ws.TypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── WorkflowStep → Department ──
            modelBuilder.Entity<WorkflowStep>()
                .HasOne(ws => ws.AssignedDepartment)
                .WithMany()
                .HasForeignKey(ws => ws.AssignedDepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── WorkflowTransition → Document ──
            modelBuilder.Entity<WorkflowTransition>()
                .HasOne(wt => wt.Document)
                .WithMany(d => d.Transitions)
                .HasForeignKey(wt => wt.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── WorkflowTransition → PerformedBy (User) ──
            modelBuilder.Entity<WorkflowTransition>()
                .HasOne(wt => wt.PerformedBy)
                .WithMany()
                .HasForeignKey(wt => wt.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── DocumentComment → Document ──
            modelBuilder.Entity<DocumentComment>()
                .HasOne(dc => dc.Document)
                .WithMany(d => d.Comments)
                .HasForeignKey(dc => dc.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── DocumentComment → User ──
            modelBuilder.Entity<DocumentComment>()
                .HasOne(dc => dc.User)
                .WithMany()
                .HasForeignKey(dc => dc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── DigitalSignature → Document ──
            modelBuilder.Entity<DigitalSignature>()
                .HasOne(ds => ds.Document)
                .WithMany(d => d.Signatures)
                .HasForeignKey(ds => ds.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── DigitalSignature → SignedBy (User) ──
            modelBuilder.Entity<DigitalSignature>()
                .HasOne(ds => ds.SignedBy)
                .WithMany()
                .HasForeignKey(ds => ds.SignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── AuditTrailEntry → Document (optional) ──
            modelBuilder.Entity<AuditTrailEntry>()
                .HasOne(a => a.Document)
                .WithMany(d => d.AuditTrail)
                .HasForeignKey(a => a.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── AuditTrailEntry → User (optional) ──
            modelBuilder.Entity<AuditTrailEntry>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── SMSNotification → Document ──
            modelBuilder.Entity<SMSNotification>()
                .HasOne(s => s.Document)
                .WithMany(d => d.Notifications)
                .HasForeignKey(s => s.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── SMSNotification → RecipientUser ──
            modelBuilder.Entity<SMSNotification>()
                .HasOne(s => s.RecipientUser)
                .WithMany()
                .HasForeignKey(s => s.RecipientUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── EscalationLog → Document ──
            modelBuilder.Entity<EscalationLog>()
                .HasOne(e => e.Document)
                .WithMany()
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── EscalationLog → ResolvedBy (User, optional) ──
            modelBuilder.Entity<EscalationLog>()
                .HasOne(e => e.ResolvedBy)
                .WithMany()
                .HasForeignKey(e => e.ResolvedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── EscalationLog → NotifiedUser ──
            modelBuilder.Entity<EscalationLog>()
                .HasOne(e => e.NotifiedUser)
                .WithMany()
                .HasForeignKey(e => e.NotifiedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── ExportAuditLog → User ──
            modelBuilder.Entity<ExportAuditLog>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── ReportLog → GeneratedBy (User) ──
            modelBuilder.Entity<ReportLog>()
                .HasOne(r => r.GeneratedBy)
                .WithMany()
                .HasForeignKey(r => r.GeneratedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── QRCodeRecord → Document ──
            modelBuilder.Entity<QRCodeRecord>()
                .HasOne(q => q.Document)
                .WithMany()
                .HasForeignKey(q => q.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ══════════════════════════════════════════════════
            // SEED DATA
            // ══════════════════════════════════════════════════

            // ── Departments ──
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, DepartmentName = "Office of the Mayor", DepartmentCode = "OM" },
                new Department { Id = 2, DepartmentName = "City Health Office", DepartmentCode = "CHO" },
                new Department { Id = 3, DepartmentName = "City Engineering Office", DepartmentCode = "CEO" },
                new Department { Id = 4, DepartmentName = "Human Resources (HRMO)", DepartmentCode = "HRMO" },
                new Department { Id = 5, DepartmentName = "City Civil Registrar", DepartmentCode = "CCR" },
                new Department { Id = 6, DepartmentName = "City Budget Office", DepartmentCode = "CBO" },
                new Department { Id = 7, DepartmentName = "City Accounting Office", DepartmentCode = "CAO" },
                new Department { Id = 8, DepartmentName = "City Legal Office", DepartmentCode = "CLO" },
                new Department { Id = 9, DepartmentName = "City Social Welfare & Development", DepartmentCode = "CSWDO" },
                new Department { Id = 10, DepartmentName = "City Disaster Risk Reduction & Management", DepartmentCode = "CDRRMO" },
                new Department { Id = 11, DepartmentName = "City Tourism Office", DepartmentCode = "CTO" },
                new Department { Id = 12, DepartmentName = "City Planning & Development", DepartmentCode = "CPDO" },
                new Department { Id = 13, DepartmentName = "Admin Division", DepartmentCode = "AD" },
                new Department { Id = 14, DepartmentName = "City Assessor's Office", DepartmentCode = "CASSO" },
                new Department { Id = 15, DepartmentName = "City Treasurer's Office", DepartmentCode = "CTRO" },
                new Department { Id = 16, DepartmentName = "City Agriculture Office", DepartmentCode = "CAGRO" },
                new Department { Id = 17, DepartmentName = "City Environment & Natural Resources", DepartmentCode = "CENRO" },
                new Department { Id = 18, DepartmentName = "City General Services", DepartmentCode = "CGS" },
                new Department { Id = 19, DepartmentName = "City Veterinary Office", DepartmentCode = "CVO" },
                new Department { Id = 20, DepartmentName = "External Client", DepartmentCode = "EXT" }
            );

            // ── Document Types ──
            modelBuilder.Entity<DocumentTypeConfig>().HasData(
                new DocumentTypeConfig { Id = 1, TypeName = "Application", TotalSteps = 5, Description = "General application form" },
                new DocumentTypeConfig { Id = 2, TypeName = "Permit", TotalSteps = 6, Description = "Permit requests requiring multiple approvals" },
                new DocumentTypeConfig { Id = 3, TypeName = "Letter", TotalSteps = 4, Description = "Official correspondence" },
                new DocumentTypeConfig { Id = 4, TypeName = "Request", TotalSteps = 5, Description = "General request documents" },
                new DocumentTypeConfig { Id = 5, TypeName = "Financial Paper", TotalSteps = 7, Description = "Financial documents requiring budget review" },
                new DocumentTypeConfig { Id = 6, TypeName = "Memorandum", TotalSteps = 4, Description = "Internal memo documents" },
                new DocumentTypeConfig { Id = 7, TypeName = "Report", TotalSteps = 4, Description = "Official reports" },
                new DocumentTypeConfig { Id = 8, TypeName = "Executive Order", TotalSteps = 5, Description = "Executive orders from the Mayor" },
                new DocumentTypeConfig { Id = 9, TypeName = "Endorsement", TotalSteps = 4, Description = "Endorsement letters" },
                new DocumentTypeConfig { Id = 10, TypeName = "Special Order", TotalSteps = 5, Description = "Special orders" },
                new DocumentTypeConfig { Id = 11, TypeName = "Administrative Order", TotalSteps = 5, Description = "Administrative orders" },
                new DocumentTypeConfig { Id = 12, TypeName = "Certificate Request", TotalSteps = 4, Description = "Certificate issuance requests" },
                new DocumentTypeConfig { Id = 13, TypeName = "Research Incentive", TotalSteps = 6, Description = "Research incentive documents with metadata" },
                new DocumentTypeConfig { Id = 14, TypeName = "Travel Order", TotalSteps = 5, Description = "Travel order approvals" },
                new DocumentTypeConfig { Id = 15, TypeName = "Leave Application", TotalSteps = 4, Description = "Employee leave applications" }
            );

            // ── Default Workflow Steps for Application type (Id=1) ──
            modelBuilder.Entity<WorkflowStep>().HasData(
                new WorkflowStep { Id = 1, TypeId = 1, StepNumber = 1, StepName = "Document Received by Clerk", AssignedDepartmentId = 1, AllowedActions = "Forward" },
                new WorkflowStep { Id = 2, TypeId = 1, StepNumber = 2, StepName = "Endorsed by Executive Admin", AssignedDepartmentId = 1, AllowedActions = "Forward, Return" },
                new WorkflowStep { Id = 3, TypeId = 1, StepNumber = 3, StepName = "Reviewed by Department", AssignedDepartmentId = null, AllowedActions = "Forward, Return" },
                new WorkflowStep { Id = 4, TypeId = 1, StepNumber = 4, StepName = "Approved by Mayor", AssignedDepartmentId = 1, AllowedActions = "Approve, Return, Reject" },
                new WorkflowStep { Id = 5, TypeId = 1, StepNumber = 5, StepName = "Released to Client", AssignedDepartmentId = 1, AllowedActions = "Complete" }
            );

            // ── Seed Data: Default Users ──
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    // Password: "admin123" — SHA256 hash
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=",
                    FullName = "System Administrator",
                    Email = "admin@trackngo.mati.gov.ph",
                    Role = UserRole.ExecutiveAdmin,
                    Department = "Office of the Mayor",
                    DepartmentId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    Username = "mayor",
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=",
                    FullName = "Hon. City Mayor",
                    Email = "mayor@trackngo.mati.gov.ph",
                    Role = UserRole.Mayor,
                    Department = "Office of the Mayor",
                    DepartmentId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 3,
                    Username = "records",
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=",
                    FullName = "Records Officer",
                    Email = "records@trackngo.mati.gov.ph",
                    Role = UserRole.RecordsOfficer,
                    Department = "Office of the Mayor",
                    DepartmentId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 4,
                    Username = "oversight",
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=",
                    FullName = "CART Oversight Officer",
                    Email = "oversight@trackngo.mati.gov.ph",
                    Role = UserRole.OversightOfficer,
                    Department = "Office of the Mayor",
                    DepartmentId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
