# PowerShell Script to Create All Role-Based View Files
# Run this script from the TrackNGo root directory

$basePath = "c:\Users\Huawei\source\repos\TrackNGo\Views"

Write-Host "Creating Role-Based View Structure..." -ForegroundColor Green

# Create folder structure
$folders = @("AdminRole", "RecordsRole", "MayorRole", "OversightRole")
foreach ($folder in $folders) {
    $folderPath = Join-Path $basePath $folder
    if (-not (Test-Path $folderPath)) {
        New-Item -ItemType Directory -Path $folderPath | Out-Null
        Write-Host "✓ Created folder: $folder" -ForegroundColor Cyan
    } else {
        Write-Host "✓ Folder exists: $folder" -ForegroundColor Yellow
    }
}

# File templates for each role
$files = @{
    "AdminRole" = @(
        "WorkflowAdmin.cshtml",
        "ARTAAdmin.cshtml",
        "UserManagementAdmin.cshtml",
        "SystemConfigAdmin.cshtml",
        "AuditTrailAdmin.cshtml"
    )
    "RecordsRole" = @(
        "DashboardRecords.cshtml",
        "DocumentRecords.cshtml",
        "NotificationRecords.cshtml",
        "TrackingRecords.cshtml",
        "DocumentIntakeRecords.cshtml"
    )
    "MayorRole" = @(
        "DashboardMayor.cshtml",
        "DocumentMayor.cshtml",
        "NotificationMayor.cshtml",
        "SignatureMayor.cshtml",
        "TrackingMayor.cshtml",
        "WorkflowMayor.cshtml",
        "ARTAMayor.cshtml"
    )
    "OversightRole" = @(
        "DashboardOversight.cshtml",
        "DocumentOversight.cshtml",
        "NotificationOversight.cshtml",
        "ARTAOversight.cshtml",
        "ReportsOversight.cshtml",
        "UserManagementOversight.cshtml",
        "AuditTrailOversight.cshtml"
    )
}

# Generic template function
function Get-ViewTemplate {
    param(
        [string]$Title,
        [string]$Role,
        [string]$Module
    )
    
    return @"
@{
    ViewData["Title"] = "$Title";
    Layout = "~/Views/Shared/_LayoutApp.cshtml";
}

<div class="page-body">
    <div class="page-header">
        <div>
            <h1 class="page-header__title">$Title</h1>
            <p class="page-header__subtitle">$Role - $Module module</p>
        </div>
    </div>

    <!-- $Role Content -->
    <div class="card" style="margin-top:20px;">
        <div class="card__header">
            <span class="card__title">$Module Overview</span>
        </div>
        <div class="card__body">
            <p>This is the $Role view for $Module.</p>
            <p style="color:var(--text-muted);">Module implementation coming soon...</p>
        </div>
    </div>
</div>
"@
}

# Create all files
$totalFiles = 0
foreach ($role in $files.Keys) {
    $roleDisplay = $role -replace "Role", ""
    foreach ($file in $files[$role]) {
        $filePath = Join-Path (Join-Path $basePath $role) $file
        
        if (-not (Test-Path $filePath)) {
            $fileName = [System.IO.Path]::GetFileNameWithoutExtension($file)
            $module = $fileName -replace $roleDisplay, ""
            
            $title = "$module - $roleDisplay"
            $content = Get-ViewTemplate -Title $title -Role $roleDisplay -Module $module
            
            $content | Out-File -FilePath $filePath -Encoding UTF8
            $totalFiles++
            Write-Host "  ✓ Created: $role\$file" -ForegroundColor Green
        } else {
            Write-Host "  - Exists: $role\$file" -ForegroundColor Gray
        }
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✓ Created $totalFiles new view files" -ForegroundColor Green
Write-Host "✓ Role-based view structure is ready!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Update controllers to route to role-specific views" -ForegroundColor White
Write-Host "2. Customize each view with role-appropriate content" -ForegroundColor White
Write-Host "3. Update _Sidebar.cshtml to link to new views" -ForegroundColor White
Write-Host "4. Test each role's access and functionality" -ForegroundColor White
