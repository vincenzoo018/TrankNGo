# TrackNGo Database Fix Script
# This script will drop and recreate the database to match the current models

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "TrackNGo Database Fix Script" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Stop if any errors
$ErrorActionPreference = "Stop"

try {
    Write-Host "[1/4] Stopping any running application processes..." -ForegroundColor Yellow
    $processes = Get-Process -Name "TrackNGo" -ErrorAction SilentlyContinue
    if ($processes) {
        $processes | Stop-Process -Force
        Write-Host "   ✓ Stopped running TrackNGo processes" -ForegroundColor Green
        Start-Sleep -Seconds 2
    } else {
        Write-Host "   ✓ No running processes found" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "[2/4] Dropping existing database..." -ForegroundColor Yellow
    dotnet ef database drop --force --context ApplicationDbContext
    Write-Host "   ✓ Database dropped successfully" -ForegroundColor Green

    Write-Host ""
    Write-Host "[3/4] Applying migrations and creating database..." -ForegroundColor Yellow
    dotnet ef database update --context ApplicationDbContext
    Write-Host "   ✓ Database created with current schema" -ForegroundColor Green

    Write-Host ""
    Write-Host "[4/4] Building application..." -ForegroundColor Yellow
    dotnet build
    Write-Host "   ✓ Build completed successfully" -ForegroundColor Green

    Write-Host ""
    Write-Host "=================================================" -ForegroundColor Green
    Write-Host "DATABASE FIX COMPLETED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Start your application (dotnet run or F5 in Visual Studio)" -ForegroundColor White
    Write-Host "2. Login with default credentials:" -ForegroundColor White
    Write-Host "   - Username: admin, mayor, records, or oversight" -ForegroundColor White
    Write-Host "   - Password: admin123" -ForegroundColor White
    Write-Host "3. Test SMS Notifications and Document Intake" -ForegroundColor White
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "=================================================" -ForegroundColor Red
    Write-Host "ERROR OCCURRED!" -ForegroundColor Red
    Write-Host "=================================================" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Make sure no application is running" -ForegroundColor White
    Write-Host "2. Close Visual Studio if it's open" -ForegroundColor White
    Write-Host "3. Try running this script again" -ForegroundColor White
    Write-Host ""
    exit 1
}
