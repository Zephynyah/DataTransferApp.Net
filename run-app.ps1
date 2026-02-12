# Stop any running instances
Write-Host "Stopping existing instances..." -ForegroundColor Yellow
Get-Process -Name "DataTransferApp.Net" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# Run the app
Write-Host "Starting application..." -ForegroundColor Green

# Option 1: Normal run (restart manually after XAML changes)
dotnet run --project ./DataTransferApp.Net\DataTransferApp.Net.csproj --configuration Release

# Option 2: Watch mode (works for C# changes, XAML requires restart)
# dotnet watch run --project ./DataTransferApp.Net\DataTransferApp.Net.csproj --configuration Debug

# Option 3: Use Visual Studio with Hot Reload for full XAML hot reload support
# Open DataTransferApp.Net.sln in Visual Studio and press F5
