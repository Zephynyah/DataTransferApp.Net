# Code Quality Check Script for Data Transfer Application
# This script runs various code quality tools similar to Rubocop

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
$warnings = $buildOutput | Select-String "warning"
$errors = $buildOutput | Select-String "error"

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
if (Test-Path "tests" -or (Get-ChildItem -Filter "*Test*.csproj" -Recurse)) {
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