# TrackNGo - Quick Start Guide 🚀

**Database:** ✅ Fixed and Ready  
**Build:** ✅ Successful  
**Status:** ✅ All systems operational

---

## Start the Application

```bash
dotnet run
```

**Application URL:** http://localhost:5259

---

## Default Login Accounts

All accounts use password: **`password123`**

### 1. Mayor Account
- **Username:** `mayor`
- **Role:** Mayor (Hon. City Mayor)
- **Access:** 7 modules
  - Dashboard
  - All Documents
  - Digital Signature
  - Workflow Routing
  - QR Tracking
  - SMS Notifications
  - ARTA Compliance

### 2. Executive Admin Account
- **Username:** `admin`
- **Role:** Executive Admin (System Administrator)
- **Access:** 9 modules
  - Dashboard
  - Document Intake
  - All Documents
  - Digital Signature
  - Workflow Routing
  - QR Tracking
  - SMS Notifications
  - User Management
  - System Configuration

### 3. Records Officer Account
- **Username:** `records`
- **Role:** Records Officer
- **Access:** 5 modules
  - Dashboard
  - Document Intake
  - My Documents
  - QR Tracking
  - Document Archive

### 4. Oversight Officer Account
- **Username:** `oversight`
- **Role:** CART/Oversight Officer
- **Access:** 7 modules
  - Dashboard
  - All Documents
  - ARTA Compliance
  - Escalation Monitor
  - Reports & Analytics
  - SMS Notifications
  - User Management

---

## Previously Fixed Issues ✅

### 1. SMS Notifications Error - RESOLVED
**Before:** `SqlException: Invalid column name 'GatewayResponse', 'MessageContent'...`  
**After:** All columns created successfully, page loads without errors

### 2. Document Intake Error - RESOLVED
**Before:** `SqlException: Invalid column name 'CategoryFlags', 'ConferenceName'...`  
**After:** DocumentMetadata table fully populated, uploads work correctly

### 3. Sidebar Role Detection Bug - RESOLVED
**Before:** All roles only showed Dashboard + All Documents  
**After:** Each role shows their authorized modules correctly

---

## Database Information

**Server:** localhost\sqlexpress  
**Database:** TrackNGoDB  
**Tables:** 17 (all created successfully)  
**Departments:** 20 seeded  
**Document Types:** 15 seeded  
**Default Users:** 4 seeded

### Connection String
```
Data Source=localhost\sqlexpress;Initial Catalog=TrackNGoDB;Integrated Security=True;Trust Server Certificate=True
```

---

## Test Checklist

After starting the application, verify:

- [ ] Login page loads at http://localhost:5259
- [ ] Can login with mayor/password123
- [ ] Mayor sees 7 modules in sidebar
- [ ] SMS Notifications page loads (no column errors)
- [ ] Can login with records/password123
- [ ] Document Intake page loads (no column errors)
- [ ] Can upload a document successfully
- [ ] All sidebar modules match role permissions

---

## Key Features

### ARTA Compliance Monitoring
- 3-day threshold: Simple transactions
- 7-day threshold: Complex transactions
- 20-day threshold: Highly technical transactions
- Smart Escalation Service runs every hour

### Document Tracking
- QR code generation for all documents
- Real-time status tracking (11 statuses)
- Digital signature support
- Routing slip management

### Multi-Role Access Control
- Role-based sidebar navigation
- Permission-based document visibility
- Secure password hashing (SHA256)
- Session-based authentication (8-hour expiry)

---

## Useful Commands

### Run the Application
```bash
dotnet run
```

### Build Only
```bash
dotnet build
```

### Database Migration Status
```bash
dotnet ef migrations list
```

### Update Database (if needed)
```bash
dotnet ef database update
```

### Create New Migration (after model changes)
```bash
dotnet ef migrations add MigrationName
```

---

## Documentation Files

- **ROLE_PROCESS_GUIDE.md** - Complete role documentation (100+ pages)
- **DATABASE_FIX_COMPLETE.md** - Database fix summary
- **SIDEBAR_MODULE_ACCESS.md** - Module access matrix
- **CHANGELOG_SIDEBAR.md** - Sidebar fix history
- **DATABASE_FIX_README.md** - Database troubleshooting guide

---

## Architecture Overview

### Technology Stack
- **Backend:** ASP.NET Core 8.0 MVC
- **Database:** SQL Server (localhost\sqlexpress)
- **ORM:** Entity Framework Core 8.0
- **Authentication:** Cookie-based (8-hour sessions)
- **Frontend:** Razor Views + Bootstrap

### Key Services
1. **AuthService** - User authentication & session management
2. **DocumentService** - Document CRUD operations
3. **WorkflowService** - Workflow state management
4. **QRCodeService** - QR code generation
5. **SMSService** - SMS notification queue
6. **SmartEscalationService** - ARTA compliance monitoring
7. **SignatureService** - Digital signature management
8. **AuditTrailService** - Activity logging
9. **ExportService** - Data export (PDF, Excel)
10. **ReportService** - Report generation

---

## Need Help?

### Common Issues

**Issue:** Can't connect to SQL Server  
**Solution:** Ensure SQL Server Express is running and accepts local connections

**Issue:** Login fails  
**Solution:** Verify default password is `password123` (case-sensitive)

**Issue:** Module not showing in sidebar  
**Solution:** Check ROLE_PROCESS_GUIDE.md for role-based access matrix

**Issue:** Build errors  
**Solution:** Run `dotnet restore` then `dotnet build`

---

## System Requirements

- .NET 8.0 SDK
- SQL Server Express (or higher)
- Windows OS (tested on Windows)
- Modern web browser (Chrome, Firefox, Edge)

---

**Status:** All systems operational ✅  
**Last Updated:** June 29, 2026  
**Version:** 1.0.0
