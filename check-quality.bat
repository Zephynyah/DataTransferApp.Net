@echo off
setlocal enabledelayedexpansion
REM Batch version of check-quality.ps1
REM Runs code quality checks and formatting for the Data Transfer Application.

REM Parse arguments
set FIX=0
set BUILD=0
set ALL=0

:parse_args
if "%1"=="" goto end_parse
if /i "%1"=="/Fix" set FIX=1
if /i "%1"=="/Build" set BUILD=1
if /i "%1"=="/All" set ALL=1
shift
goto parse_args
:end_parse

echo Running Code Quality Checks...
echo =====================================

REM Change to the project directory
REM cd /d d:\Powershell\GUI\DTA-WPF\DataTransferApp.Net

if %BUILD%==1 (
    echo.
    echo Building project ^(this triggers Roslyn analyzers^)...
    dotnet build --verbosity quiet
    if errorlevel 1 (
        echo Build failed! Fix compilation errors first.
        exit /b 1
    )
    echo Build successful
)

if %ALL%==1 (
    echo.
    echo Building project ^(this triggers Roslyn analyzers^)...
    dotnet build --verbosity quiet
    if errorlevel 1 (
        echo Build failed! Fix compilation errors first.
        exit /b 1
    )
    echo Build successful
)

REM Run dotnet format
if %FIX%==1 (
    echo.
    echo Formatting code with dotnet-format...
    dotnet format
    echo Code formatted
)

if %ALL%==1 (
    echo.
    echo Formatting code with dotnet-format...
    dotnet format
    echo Code formatted
)

REM Run analyzers explicitly
echo.
echo Running Roslyn analyzers...
dotnet build /p:RunAnalyzersDuringBuild=true > analyzer_output.txt 2>&1
findstr "warning" analyzer_output.txt | findstr /v "Warning(s)" > warnings.txt
findstr "error" analyzer_output.txt | findstr /v "Error(s)" > errors.txt

REM Count warnings and errors
if exist warnings.txt (
    for /f %%i in ('type warnings.txt ^| find /c /v ""') do set WARNINGS=%%i
) else (
    set WARNINGS=0
)
if exist errors.txt (
    for /f %%i in ('type errors.txt ^| find /c /v ""') do set ERRORS=%%i
) else (
    set ERRORS=0
)

REM Display warnings and errors
echo.
echo Analysis Results:
echo Warnings found: !WARNINGS!
echo Errors found: !ERRORS!

if !WARNINGS! gtr 0 (
    echo.
    echo Warnings:
    type warnings.txt
)

if !ERRORS! gtr 0 (
    echo.
    echo Errors:
    type errors.txt
    del analyzer_output.txt warnings.txt errors.txt
    exit /b 1
)

REM Run tests if they exist
if exist "tests" (
    echo.
    echo Running tests...
    dotnet test --verbosity quiet
    if errorlevel 1 (
        echo Tests failed!
        del analyzer_output.txt warnings.txt errors.txt
        exit /b 1
    )
    echo Tests passed
) else (
    REM Check for test projects
    dir /b /s "*Test*.csproj" 2>nul | findstr . >nul
    if not errorlevel 1 (
        echo.
        echo Running tests...
        dotnet test --verbosity quiet
        if errorlevel 1 (
            echo Tests failed!
            del analyzer_output.txt warnings.txt errors.txt
            exit /b 1
        )
        echo Tests passed
    )
)

del analyzer_output.txt warnings.txt errors.txt

echo.
echo Code quality checks completed!

if %ALL%==1 (
    echo.
    echo Tips:
    echo   - Use 'dotnet format' to auto-fix formatting issues
    echo   - Check .editorconfig for style rules
    echo   - Analyzers run automatically in Visual Studio/VS Code
    echo   - Consider adding pre-commit hooks with Husky.Net
)