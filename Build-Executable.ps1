<#
.Synopsis
   Builds the Data Transfer Application as a standalone executable.
.DESCRIPTION
   This script publishes the .NET WPF application as a standalone executable.
   It supports different runtimes, configurations, and options for single-file and self-contained builds.
   The output executable will be placed in the "publish" directory under a subfolder named with the runtime and configuration.
   The script also cleans the previous build output before publishing the new executable.
.PARAMETER Runtime
   The target runtime for the published executable. Valid values are "win-x64", "win-x86", and "win-arm64".
.PARAMETER Configuration
   The build configuration for the published executable. Valid values are "Debug" and "Release".
.PARAMETER SingleFile
   Indicates whether to publish the application as a single file.
.PARAMETER SelfContained
   Indicates whether to publish the application as self-contained.
.PARAMETER Trimmed
   Indicates whether to enable trimming of unused assemblies to reduce the size of the published application.
.EXAMPLE
   .\build-exe.ps1 -Runtime win-x64 -Configuration Release -SingleFile -SelfContained -Trimmed
.EXAMPLE
   .\build-exe.ps1 -Runtime win-x64 -Configuration Debug
.INPUTS
   None
.OUTPUTS
   None
.NOTES
   This script requires the .NET SDK to be installed and available in the system PATH.
   The script is intended for building the Data Transfer Application as a standalone executable for Windows platforms.
   The script cleans the previous build output before publishing the new executable.
.COMPONENT
   Data Transfer Application
.ROLE
   Build
.FUNCTIONALITY
   Publishes the .NET WPF application as a standalone executable
#>
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("win-x64", "win-x86", "win-arm64")]
    [string]$Runtime = "win-x64",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [switch]$SingleFile,
    
    [Parameter(Mandatory=$false)]
    [switch]$SelfContained,
    
    [Parameter(Mandatory=$false)]
    [switch]$Trimmed
)

$ErrorActionPreference = "Stop"

# Script paths
$ScriptRoot = $PSScriptRoot
$ProjectPath = Join-Path $ScriptRoot "DataTransferApp.Net\DataTransferApp.Net.csproj"
$OutputPath = Join-Path $ScriptRoot "publish\$Runtime-$Configuration"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Data Transfer Application" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  - Runtime:        $Runtime" -ForegroundColor White
Write-Host "  - Configuration:  $Configuration" -ForegroundColor White
Write-Host "  - Single File:    $SingleFile" -ForegroundColor White
Write-Host "  - Self-Contained: $SelfContained" -ForegroundColor White
Write-Host "  - Trimmed:        $Trimmed" -ForegroundColor White
Write-Host "  - Output Path:    $OutputPath" -ForegroundColor White
Write-Host ""

# Verify project file exists
if (-not (Test-Path $ProjectPath)) {
    Write-Host "ERROR: Project file not found at: $ProjectPath" -ForegroundColor Red
    exit 1
}

# Clean previous publish folder
if (Test-Path $OutputPath) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item $OutputPath -Recurse -Force
}

# Create output directory
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Build publish arguments
$publishArgs = @(
    "publish"
    $ProjectPath
    "--configuration", $Configuration
    "--runtime", $Runtime
    "--output", $OutputPath
)

if ($SelfContained) {
    $publishArgs += "--self-contained", "true"
} else {
    $publishArgs += "--self-contained", "false"
}

if ($SingleFile) {
    $publishArgs += "-p:PublishSingleFile=true"
    $publishArgs += "-p:IncludeNativeLibrariesForSelfExtract=true"
}

if ($Trimmed) {
    $publishArgs += "-p:PublishTrimmed=true"
    $publishArgs += "-p:TrimMode=link"
}

# Additional optimizations for release builds
if ($Configuration -eq "Release") {
    $publishArgs += "-p:DebugType=None"
    $publishArgs += "-p:DebugSymbols=false"
}

# Execute publish command
Write-Host "Publishing application..." -ForegroundColor Yellow
Write-Host "Command: dotnet $($publishArgs -join ' ')" -ForegroundColor Gray
Write-Host ""

try {
    & dotnet $publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Build Successful!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    
    # Show output details
    $exeName = "DataTransferApp.Net.exe"
    $exePath = Join-Path $OutputPath $exeName
    
    if (Test-Path $exePath) {
        $exeInfo = Get-Item $exePath
        $sizeInMB = [math]::Round($exeInfo.Length / 1MB, 2)
        
        Write-Host "Executable Information:" -ForegroundColor Cyan
        Write-Host "  - Name:     $($exeInfo.Name)" -ForegroundColor White
        Write-Host "  - Size:     $sizeInMB MB" -ForegroundColor White
        Write-Host "  - Location: $($exeInfo.FullName)" -ForegroundColor White
        Write-Host ""
        
        # List all files in output
        Write-Host "Published Files:" -ForegroundColor Cyan
        Get-ChildItem $OutputPath | ForEach-Object {
            $size = if ($_.PSIsContainer) { "Folder" } else { "$([math]::Round($_.Length / 1KB, 2)) KB" }
            Write-Host "  - $($_.Name) ($size)" -ForegroundColor Gray
        }
        Write-Host ""
        
        # Copy config if exists
        $configSource = Join-Path $ScriptRoot "DataTransferApp.Net\appsettings.json"
        $configDest = Join-Path $OutputPath "appsettings.json"
        if ((Test-Path $configSource) -and -not (Test-Path $configDest)) {
            Write-Host "Copying configuration file..." -ForegroundColor Yellow
            Copy-Item $configSource $configDest -Force
        }
        
        Write-Host "To run the application:" -ForegroundColor Yellow
        Write-Host "  $exePath" -ForegroundColor White
        Write-Host ""
        
        # Offer to open folder
        $response = Read-Host "Open output folder? (Y/N)"
        if ($response -eq "Y" -or $response -eq "y") {
            Start-Process explorer.exe $OutputPath
        }
        
    } else {
        Write-Host "WARNING: Expected executable not found at: $exePath" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host ""
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
