@echo off
REM TrackNGo Database Fix Script (Windows Batch Version)
REM This script will drop and recreate the database to match the current models

echo ================================================
echo TrackNGo Database Fix Script
echo ================================================
echo.

echo [1/4] Stopping any running application processes...
taskkill /F /IM TrackNGo.exe /T 2>nul
if %errorlevel%==0 (
    echo    [OK] Stopped running TrackNGo processes
    timeout /t 2 /nobreak >nul
) else (
    echo    [OK] No running processes found
)

echo.
echo [2/4] Dropping existing database...
dotnet ef database drop --force --context ApplicationDbContext
if %errorlevel% neq 0 (
    echo    [ERROR] Failed to drop database
    goto error
)
echo    [OK] Database dropped successfully

echo.
echo [3/4] Applying migrations and creating database...
dotnet ef database update --context ApplicationDbContext
if %errorlevel% neq 0 (
    echo    [ERROR] Failed to create database
    goto error
)
echo    [OK] Database created with current schema

echo.
echo [4/4] Building application...
dotnet build
if %errorlevel% neq 0 (
    echo    [ERROR] Build failed
    goto error
)
echo    [OK] Build completed successfully

echo.
echo ================================================
echo DATABASE FIX COMPLETED SUCCESSFULLY!
echo ================================================
echo.
echo Next steps:
echo 1. Start your application (dotnet run or F5 in Visual Studio)
echo 2. Login with default credentials:
echo    - Username: admin, mayor, records, or oversight
echo    - Password: admin123
echo 3. Test SMS Notifications and Document Intake
echo.
pause
exit /b 0

:error
echo.
echo ================================================
echo ERROR OCCURRED!
echo ================================================
echo.
echo Troubleshooting:
echo 1. Make sure no application is running
echo 2. Close Visual Studio if it's open
echo 3. Try running this script again
echo.
pause
exit /b 1
