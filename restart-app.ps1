# SCRIPT DE KHOI DONG LAI APP
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RESTART EV SERVICE CENTER API" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Kill tat ca process dotnet
Write-Host "[1/3] Stopping all dotnet processes..." -ForegroundColor Yellow
Get-Process -Name dotnet -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Write-Host "Done!" -ForegroundColor Green
Write-Host ""

# 2. Clean build artifacts
Write-Host "[2/3] Cleaning build artifacts..." -ForegroundColor Yellow
Set-Location "C:\Users\ADMIN\Desktop\MyProject\EVServiceCenter-BackupV2-Done-main\EVServiceCenter-BackupV2-Done-main\EVServiceCenter.API"
dotnet clean --verbosity quiet
Start-Sleep -Seconds 1
Write-Host "Done!" -ForegroundColor Green
Write-Host ""

# 3. Start app
Write-Host "[3/3] Starting application..." -ForegroundColor Yellow
Write-Host "Listening on http://localhost:5153" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Gray
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run
