<#
.SYNOPSIS
    Runs code quality checks and formatting for the Data Transfer Application.
.DESCRIPTION
    This PowerShell script performs code quality checks using Roslyn analyzers,
    formats code with dotnet-format, and runs tests if available.
.PARAMETER Fix
    If specified, the script will attempt to fix formatting issues.
.PARAMETER Build
    If specified, the script will build the project to trigger analyzers.
.PARAMETER All
    If specified, the script will run all checks including formatting and building.
.EXAMPLE
    .\check-quality.ps1 -Fix
    Runs the code quality checks and attempts to fix formatting issues.
.EXAMPLE
    .\check-quality.ps1 -Build
    Runs the code quality checks and builds the project to trigger analyzers.
.EXAMPLE
    .\check-quality.ps1 -All
    Runs all code quality checks including formatting and building.
.NOTES
    This script requires the .NET SDK to be installed.
    It uses Roslyn analyzers, dotnet-format, and dotnet test.
    Ensure you have the necessary permissions to run these commands.
.INPUTS
#>
param(
    [switch]$Fix,
    [switch]$Build,
    [switch]$All
)

Write-Host "🔍 Running Code Quality Checks..." -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Change to the project directory
Set-Location $PSScriptRoot

# Run dotnet build to trigger analyzers
if ($Build -or $All) {
    Write-Host "`n🏗️  Building project (this triggers Roslyn analyzers)..." -ForegroundColor Yellow
    dotnet build --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed! Fix compilation errors first." -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Build successful" -ForegroundColor Green
}

# Run dotnet format
if ($Fix -or $All) {
    Write-Host "`n🎨 Formatting code with dotnet-format..." -ForegroundColor Yellow
    dotnet format
    Write-Host "✅ Code formatted" -ForegroundColor Green
}

# Run analyzers explicitly
Write-Host "`n🔬 Running Roslyn analyzers..." -ForegroundColor Yellow
dotnet build /p:RunAnalyzersDuringBuild=true --verbosity quiet

# Check for warnings/errors
$buildOutput = dotnet build 2>&1
$warnings = $buildOutput | Select-String ":\s*warning\s" | Where-Object { $_ -notmatch "^\s*\d+\s+Warning\(s\)$" }
$errors = $buildOutput | Select-String ":\s*error\s" | Where-Object { $_ -notmatch "^\s*\d+\s+Error\(s\)$" }

Write-Host "`n📊 Analysis Results:" -ForegroundColor Cyan
Write-Host "Warnings found: $($warnings.Count)" -ForegroundColor Yellow
Write-Host "Errors found: $($errors.Count)" -ForegroundColor Red

if ($warnings.Count -gt 0) {
    Write-Host "`n⚠️  Warnings:" -ForegroundColor Yellow
    $warnings | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}

if ($errors.Count -gt 0) {
    Write-Host "`n❌ Errors:" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}

# Run tests if they exist
if ((Test-Path "tests") -or (Get-ChildItem -Filter "*Test*.csproj" -Recurse)) {
    Write-Host "`n🧪 Running tests..." -ForegroundColor Yellow
    dotnet test --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tests failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Tests passed" -ForegroundColor Green
}

Write-Host "`n🎉 Code quality checks completed!" -ForegroundColor Green

if ($All) {
    Write-Host "`n💡 Tips:" -ForegroundColor Cyan
    Write-Host "  - Use 'dotnet format' to auto-fix formatting issues" -ForegroundColor White
    Write-Host "  - Check .editorconfig for style rules" -ForegroundColor White
    Write-Host "  - Analyzers run automatically in Visual Studio/VS Code" -ForegroundColor White
    Write-Host "  - Consider adding pre-commit hooks with Husky.Net" -ForegroundColor White
}