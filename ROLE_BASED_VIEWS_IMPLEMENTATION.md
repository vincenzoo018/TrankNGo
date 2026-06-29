# Role-Based Views Implementation Guide

**Status:** In Progress  
**Created:** June 29, 2026  
**Total Files to Create:** 40 view files across 4 roles

---

## FOLDER STRUCTURE

```
Views/
├── AdminRole/          (10 files) ✅ STARTED
│   ├── DashboardAdmin.cshtml          ✅ CREATED
│   ├── DocumentAdmin.cshtml           ✅ CREATED
│   ├── NotificationAdmin.cshtml       ✅ CREATED
│   ├── SignatureAdmin.cshtml          ✅ CREATED
│   ├── TrackingAdmin.cshtml           ⏳ TODO
│   ├── WorkflowAdmin.cshtml           ⏳ TODO
│   ├── ARTAAdmin.cshtml               ⏳ TODO
│   ├── UserManagementAdmin.cshtml     ⏳ TODO
│   ├── SystemConfigAdmin.cshtml       ⏳ TODO
│   └── AuditTrailAdmin.cshtml         ⏳ TODO
│
├── RecordsRole/        (5 files) ⏳ TODO
│   ├── DashboardRecords.cshtml
│   ├── DocumentRecords.cshtml
│   ├── NotificationRecords.cshtml
│   ├── TrackingRecords.cshtml
│   └── DocumentIntakeRecords.cshtml
│
├── MayorRole/          (7 files) ⏳ TODO
│   ├── DashboardMayor.cshtml
│   ├── DocumentMayor.cshtml
│   ├── NotificationMayor.cshtml
│   ├── SignatureMayor.cshtml
│   ├── TrackingMayor.cshtml
│   ├── WorkflowMayor.cshtml
│   └── ARTAMayor.cshtml
│
└── OversightRole/      (7 files) ⏳ TODO
    ├── DashboardOversight.cshtml
    ├── DocumentOversight.cshtml
    ├── NotificationOversight.cshtml
    ├── ARTAOversight.cshtml
    ├── ReportsOversight.cshtml
    ├── UserManagementOversight.cshtml
    └── AuditTrailOversight.cshtml
```

---

## IMPLEMENTATION APPROACH

### Phase 1: Create Remaining Admin Files (6 files)
- TrackingAdmin.cshtml
- WorkflowAdmin.cshtml
- ARTAAdmin.cshtml
- UserManagementAdmin.cshtml (clone from Admin/Index.cshtml)
- SystemConfigAdmin.cshtml
- AuditTrailAdmin.cshtml

### Phase 2: Create Records Officer Files (5 files)
- DashboardRecords.cshtml
- DocumentRecords.cshtml
- NotificationRecords.cshtml
- TrackingRecords.cshtml
- DocumentIntakeRecords.cshtml

### Phase 3: Create Mayor Files (7 files)
- DashboardMayor.cshtml
- DocumentMayor.cshtml
- NotificationMayor.cshtml
- SignatureMayor.cshtml
- TrackingMayor.cshtml
- WorkflowMayor.cshtml
- ARTAMayor.cshtml

### Phase 4: Create Oversight Officer Files (7 files)
- DashboardOversight.cshtml
- DocumentOversight.cshtml
- NotificationOversight.cshtml
- ARTAOversight.cshtml
- ReportsOversight.cshtml
- UserManagementOversight.cshtml
- AuditTrailOversight.cshtml

---

## CONTROLLER ROUTING UPDATE NEEDED

After creating all view files, update controllers to route to role-specific views:

### Example: DashboardController.cs
```csharp
public IActionResult Index()
{
    var role = GetCurrentUserRole(); // From BaseController
    
    var viewName = role switch
    {
        UserRole.Mayor => "~/Views/MayorRole/DashboardMayor.cshtml",
        UserRole.RecordsOfficer => "~/Views/RecordsRole/DashboardRecords.cshtml",
        UserRole.OversightOfficer => "~/Views/OversightRole/DashboardOversight.cshtml",
        UserRole.ExecutiveAdmin => "~/Views/AdminRole/DashboardAdmin.cshtml",
        _ => "Index" // Default
    };
    
    return View(viewName, model);
}
```

### Controllers That Need Updates:
1. **DashboardController** - Index action
2. **DocumentController** - Index, Create actions
3. **NotificationController** - Index action
4. **SignatureController** - Index action
5. **TrackingController** - Index action
6. **WorkflowController** - Index action
7. **OversightController** - Index, Reports actions
8. **AdminController** - Index, Audit actions

---

## DESIGN CONSISTENCY GUIDELINES

### 1. Page Header Format
```cshtml
<div class="page-header">
    <div>
        <h1 class="page-header__title">[Module Name]</h1>
        <p class="page-header__subtitle">[Role Name] - [Description]</p>
    </div>
    <!-- Action Buttons -->
</div>
```

### 2. Role-Specific Color Scheme
- **Mayor:** Primary Blue (#0F2A6B)
- **Executive Admin:** Dark Blue (#1E293B)
- **Records Officer:** Teal (#0D9488)
- **Oversight Officer:** Orange (#EA580C)

### 3. Alert Messages by Role
```cshtml
<!-- Admin -->
<div class="alert alert--info">
    <strong>Admin Access:</strong> You have full system access...
</div>

<!-- Records -->
<div class="alert alert--info">
    <strong>Records View:</strong> You see documents you created...
</div>

<!-- Mayor -->
<div class="alert alert--warning">
    <strong>Executive Priority:</strong> Documents requiring your attention...
</div>

<!-- Oversight -->
<div class="alert alert--danger">
    <strong>ARTA Alert:</strong> Monitor compliance violations...
</div>
```

### 4. Stat Cards Customization
Each role should have relevant metrics:

**Mayor:**
- Pending Signature
- ARTA Flagged
- Returned Documents
- Total Documents

**Admin:**
- Newly Submitted
- In Progress
- Pending Mayor
- Total Documents

**Records:**
- Received Today
- Awaiting Resubmission
- Ready for Release
- My Documents

**Oversight:**
- Warnings
- Critical
- Overdue
- Compliance Rate

---

## SIDEBAR NAVIGATION UPDATE

Update `_Sidebar.cshtml` to link to role-specific views:

```cshtml
@if (role == TrackNGo.Models.UserRole.ExecutiveAdmin)
{
    <a href="/AdminRole/DashboardAdmin" class="sidebar__link">
        <span>Dashboard</span>
    </a>
}
@if (role == TrackNGo.Models.UserRole.Mayor)
{
    <a href="/MayorRole/DashboardMayor" class="sidebar__link">
        <span>Dashboard</span>
    </a>
}
@if (role == TrackNGo.Models.UserRole.RecordsOfficer)
{
    <a href="/RecordsRole/DashboardRecords" class="sidebar__link">
        <span>Dashboard</span>
    </a>
}
@if (role == TrackNGo.Models.UserRole.OversightOfficer)
{
    <a href="/OversightRole/DashboardOversight" class="sidebar__link">
        <span>Dashboard</span>
    </a>
}
```

---

## VIEWMODEL CUSTOMIZATION

Some views need role-specific ViewModels:

### Create New ViewModels:
1. **MayorDashboardViewModel** - Mayor-specific metrics
2. **RecordsDashboardViewModel** - Records-specific metrics
3. **OversightDashboardViewModel** - Oversight-specific metrics
4. **DocumentListByRoleViewModel** - Filtered by role permissions

---

## TESTING CHECKLIST

After implementation, test each role:

### Executive Admin Testing
- [ ] Dashboard loads with admin metrics
- [ ] Can see ALL documents
- [ ] Can access User Management
- [ ] Can access System Configuration
- [ ] Can send SMS notifications
- [ ] Can view audit trail
- [ ] Can manage digital signatures
- [ ] Can track documents via QR
- [ ] Can view ARTA compliance
- [ ] Can manage workflows

### Records Officer Testing
- [ ] Dashboard shows intake metrics
- [ ] Can see only own documents
- [ ] Can submit new documents
- [ ] Can track via QR code
- [ ] Can receive SMS notifications
- [ ] NO access to admin functions
- [ ] NO access to mayor functions

### Mayor Testing
- [ ] Dashboard shows approval metrics
- [ ] Can see all documents
- [ ] Can digitally sign documents
- [ ] Can view ARTA compliance
- [ ] Can manage workflows
- [ ] Can track documents
- [ ] NO access to admin config
- [ ] NO access to user management

### Oversight Officer Testing
- [ ] Dashboard shows ARTA metrics
- [ ] Can see all documents
- [ ] Can view escalations
- [ ] Can generate reports
- [ ] Can export data
- [ ] Can view audit trail
- [ ] Can manage users
- [ ] NO access to system config

---

## QUICK CREATE SCRIPT

To speed up creation, use this PowerShell script:

```powershell
# Create all role folders
$roles = @("AdminRole", "RecordsRole", "MayorRole", "OversightRole")
$basePath = "c:\Users\Huawei\source\repos\TrackNGo\Views"

foreach ($role in $roles) {
    $rolePath = Join-Path $basePath $role
    if (-not (Test-Path $rolePath)) {
        New-Item -ItemType Directory -Path $rolePath
        Write-Host "Created folder: $rolePath"
    }
}
```

---

## COMPLETION STATUS

- **Admin Role:** 4/10 files ✅ (40%)
- **Records Role:** 0/5 files ⏳ (0%)
- **Mayor Role:** 0/7 files ⏳ (0%)
- **Oversight Role:** 0/7 files ⏳ (0%)

**Overall Progress:** 4/29 files (14%)

---

## NEXT STEPS

1. ✅ Complete remaining AdminRole files
2. ⏳ Create all RecordsRole files
3. ⏳ Create all MayorRole files
4. ⏳ Create all OversightRole files
5. ⏳ Update all controllers to route to role-specific views
6. ⏳ Update sidebar navigation
7. ⏳ Test all roles thoroughly
8. ⏳ Create role-specific ViewModels if needed

---

**Would you like me to continue creating the remaining files?**
