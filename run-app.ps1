# Stop any running instances
Write-Host "Stopping existing instances..." -ForegroundColor Yellow
Get-Process -Name "DataTransferApp.Net" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# Run the app
Write-Host "Starting application..." -ForegroundColor Green
dotnet run --project ./DataTransferApp.Net\DataTransferApp.Net.csproj
