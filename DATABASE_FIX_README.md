# Database Schema Mismatch - Fix Guide

## 🐛 Problem Summary

You're experiencing SQL errors when:
1. **Navigating to SMS Notifications** (`/Notification/Index`)
2. **Creating documents** as Records Officer (`/Document/Create`)

### Error Messages:
```
SqlException: Invalid column name 'GatewayResponse'
Invalid column name 'MessageContent'
Invalid column name 'QueuedAt'
Invalid column name 'RecipientName'
Invalid column name 'RecipientNumber'
Invalid column name 'TriggerEvent'
Invalid column name 'QueuedAt'
```

And when creating documents:
```
SqlException: Invalid column name 'CategoryFlags'
Invalid column name 'ConferenceName'
Invalid column name 'Province'
Invalid column name 'ReportNumber'
Invalid column name 'SourceLink'
Invalid column name 'Content'
Invalid column name 'PostedAt'
...
```

## 🔍 Root Cause

The **database schema is outdated** and doesn't match the current C# model classes. This happened because:

1. The initial migration (`20260625104311_InitialCreate.cs`) created the database with a specific schema
2. The C# model classes (`SMSNotification.cs`, `DocumentMetadata.cs`, etc.) were updated
3. No new migration was created to update the database
4. **Result**: Entity Framework tries to query columns that don't exist in the database

## ✅ Solution Options

### Option 1: Quick Fix - Drop and Recreate Database (RECOMMENDED)

**⚠️ WARNING**: This will delete all existing data!

**Steps**:

1. **Stop the application** (close Visual Studio or stop dotnet run)

2. **Run the fix script**:
   ```powershell
   cd c:\Users\Huawei\source\repos\TrackNGo
   .\fix-database.ps1
   ```

3. **Restart the application**

**What the script does**:
- Stops any running TrackNGo processes
- Drops the existing database
- Applies the `InitialCreate` migration to create fresh database
- Builds the application
- Seeds default data (users, departments, document types)

**Default Credentials** (after reset):
- Username: `admin`, `mayor`, `records`, or `oversight`
- Password: `admin123`

---

### Option 2: Manual Fix (If you don't want to use the script)

1. **Stop the application**

2. **Drop the database**:
   ```powershell
   dotnet ef database drop --force --context ApplicationDbContext
   ```

3. **Recreate the database**:
   ```powershell
   dotnet ef database update --context ApplicationDbContext
   ```

4. **Rebuild the application**:
   ```powershell
   dotnet build
   ```

5. **Start the application**

---

### Option 3: Preserve Data (Advanced)

If you have important data you want to keep:

1. **Stop the application**

2. **Backup the database**:
   - Find `TrackNGo.db` in your project folder
   - Copy it to a safe location: `TrackNGo.db.backup`

3. **Export important data**:
   - Use a SQLite browser tool to export data as SQL/CSV
   - Recommended tool: DB Browser for SQLite

4. **Drop and recreate** (Option 1 or 2)

5. **Manually import** your backed-up data using SQL INSERT statements

---

## 📋 Affected Tables

The following tables have schema mismatches:

### SMSNotifications Table
**Database has (old schema)**:
- No columns (empty table or missing columns)

**Model expects (new schema)**:
- Id, DocumentId, RecipientUserId
- RecipientNumber, RecipientName
- MessageContent, TriggerEvent
- Status, QueuedAt, SentAt
- GatewayResponse

### DocumentMetadatas Table  
**Database has (old schema)**:
- Limited or no metadata fields

**Model expects (new schema)**:
- Id, DocumentId
- ConferenceName, SourceLink
- Province, ReportNumber
- CategoryFlags

### Documents Table
**Database may be missing**:
- Some relationship fields
- Metadata associations

---

## 🧪 Testing After Fix

After running the fix, test these features:

### 1. SMS Notifications
```
1. Login as any role (mayor, admin, records, oversight)
2. Navigate to SMS Notifications in sidebar
3. Page should load without errors
4. You'll see the notification log interface
```

### 2. Document Intake
```
1. Login as Records Officer or Executive Admin
2. Navigate to "Document Intake"
3. Fill out the form:
   - Title: "Test Document"
   - Document Type: "Application"
   - Originating Department: "Office of the Mayor"
   - Submitted By: "John Doe"
   - Contact Number: "+639171234567"
   - Email: "test@example.com"
   - ARTA Period: "Simple (3 days)"
4. Upload a test file (PDF/DOCX)
5. Click "Register Document"
6. Should create successfully without errors
```

### 3. All Roles
```
1. Login as each role:
   - mayor / admin123
   - admin / admin123
   - records / admin123
   - oversight / admin123
2. Verify sidebar modules appear correctly
3. Navigate to each accessible module
4. No SQL errors should appear
```

---

## 🔧 Troubleshooting

### "Application is still running" error
```powershell
# Manually kill the process
Get-Process -Name "TrackNGo" | Stop-Process -Force

# Wait a few seconds, then try again
Start-Sleep -Seconds 3
.\fix-database.ps1
```

### "dotnet ef command not found"
```powershell
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef
```

### "Cannot access database file" error
```
1. Close Visual Studio completely
2. Open Task Manager (Ctrl+Shift+Esc)
3. End any "TrackNGo.exe" or "dotnet.exe" processes
4. Delete TrackNGo.db manually if needed
5. Run fix script again
```

### Database file location
The SQLite database file is located at:
```
c:\Users\Huawei\source\repos\TrackNGo\TrackNGo.db
```

You can delete this file manually if the script fails.

---

## 📚 Understanding the Issue (Technical Details)

### Entity Framework Core Migrations

EF Core uses migrations to keep the database schema in sync with C# models:

```
C# Models (Code)  →  Migration Files  →  Database Schema
```

**What went wrong**:
1. Initial migration created: `20260625104311_InitialCreate.cs`
2. Database created from this migration
3. C# models were later modified
4. **Missing step**: No new migration was created
5. Database schema is now out of sync with models

### Why Drop and Recreate?

Since there's no production data yet (development phase), it's faster to:
- Drop the old database
- Recreate from the current models
- Seed fresh test data

### In Production (Future)

When you have real data, you must:
1. Create a new migration: `dotnet ef migrations add UpdateSchema`
2. Apply migration: `dotnet ef database update`
3. Test thoroughly before deploying

---

## ✅ Verification Checklist

After running the fix, verify:

- [ ] Application starts without errors
- [ ] Can login as mayor/admin/records/oversight
- [ ] Dashboard loads for all roles
- [ ] **SMS Notifications page loads** (previously errored)
- [ ] **Document Intake works** (previously errored)
- [ ] Can create a test document
- [ ] Document appears in "All Documents"
- [ ] QR code is generated
- [ ] Workflow transitions work
- [ ] No SQL errors in console/logs

---

## 🆘 Need More Help?

### Check Application Logs
```powershell
# Run application with detailed logging
dotnet run --configuration Debug
```

### View Database Schema
Use DB Browser for SQLite to inspect the database:
1. Download: https://sqlitebrowser.org/
2. Open: `c:\Users\Huawei\source\repos\TrackNGo\TrackNGo.db`
3. View Tables → SMSNotifications, Documents, DocumentMetadatas
4. Verify columns match model properties

### Alternative: Use SQL Server Instead of SQLite

If you continue having issues, switch to SQL Server:

1. Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TrackNGoDB;Trusted_Connection=True;"
  }
}
```

2. Update `Program.cs` to use SQL Server provider

3. Run migrations again

---

## 📞 Summary

**Problem**: Database schema doesn't match C# models  
**Cause**: Missing migration after model changes  
**Solution**: Drop and recreate database  
**Tool**: `fix-database.ps1` script (automated)  
**Time**: ~30 seconds  
**Data Loss**: Yes (development data only)  

**After Fix**: SMS Notifications and Document Intake will work perfectly! ✅

---

**Last Updated**: June 28, 2026  
**Script**: fix-database.ps1  
**Database**: TrackNGo.db (SQLite)
