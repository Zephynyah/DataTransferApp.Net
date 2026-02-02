# Build Configuration Guide

## Overview

The Data Transfer Application supports build-time configuration using conditional compilation directives. Different values are automatically used based on the build configuration (Debug vs Release).

## Build-Time Directory Configuration

The following directories are automatically configured based on build type:

### Debug Build (Development/Testing)

```csharp
StagingDirectory:         D:\Powershell\GUI\DTA\test-data\TransferStaging
RetentionDirectory:       D:\Powershell\GUI\DTA\test-data\TransferRetention
TransferRecordsDirectory: D:\Powershell\GUI\DTA\test-data\TransferRecords
```

### Release Build (Production)

```csharp
StagingDirectory:         \\Puszbf0a\GSC_FILE_TRANSFER
RetentionDirectory:       \\Puszbf0a\GSC_FILE_TRANSFER\Moved
TransferRecordsDirectory: \\Puszbf0a\GSC2\GSC_ACC\AFT\Collateral AFT Records
```

## Building for Different Environments

### Debug Build (Development)

Uses local test directories:

```powershell
.\Build-Executable.ps1 -Configuration Debug
```

### Release Build (Production)

Uses production network paths:

```powershell
.\Build-Executable.ps1 -Configuration Release -SingleFile -SelfContained
```

### All Build Options

```powershell
# Full production build with all optimizations
.\Build-Executable.ps1 -Runtime win-x64 -Configuration Release -SingleFile -SelfContained -Trimmed

# Debug build for testing
.\Build-Executable.ps1 -Runtime win-x64 -Configuration Debug

# Release build for different architecture
.\Build-Executable.ps1 -Runtime win-arm64 -Configuration Release -SelfContained
```

## Customizing Default Paths

To change the default paths for different environments, edit the conditional compilation directives in:
**`DataTransferApp.Net\Models\AppSettings.cs`**

Example:

```csharp
#if DEBUG
    private string _stagingDirectory = @"D:\Powershell\GUI\DTA\test-data\TransferStaging";
#else
    private string _stagingDirectory = @"\\Puszbf0a\GSC_FILE_TRANSFER";
#endif
```

## Creating Custom Build Configurations

### Option 1: Add Custom Preprocessor Symbols

Edit `DataTransferApp.Net.csproj`:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Staging'">
  <DefineConstants>STAGING</DefineConstants>
</PropertyGroup>
```

Then use in code:

```csharp
#if DEBUG
    private string _stagingDirectory = @"D:\Local\Test";
#elif STAGING
    private string _stagingDirectory = @"\\TestServer\Staging";
#else
    private string _stagingDirectory = @"\\Puszbf0a\GSC_FILE_TRANSFER";
#endif
```

Build with:

```powershell
dotnet build -c Staging
```

### Option 2: Environment Variables at Build Time

Set MSBuild properties:

```powershell
$env:StagingPath = "\\MyServer\MyPath"
dotnet publish -p:StagingDirectory=$env:StagingPath
```

### Option 3: Settings File Override

The app always checks for runtime settings in the database, which override build-time defaults. Users can change paths in Settings UI after deployment.

## Runtime Configuration Priority

1. **User Settings** (stored in database) - highest priority
2. **Build-Time Defaults** (conditional compilation)
3. **Hardcoded Defaults** (fallback)

## Build Parameters

| Parameter | Values | Description |
|-----------|--------|-------------|
| `-Runtime` | `win-x64`, `win-x86`, `win-arm64` | Target platform |
| `-Configuration` | `Debug`, `Release` | Build configuration |
| `-SingleFile` | Switch | Bundles all files into single .exe |
| `-SelfContained` | Switch | Includes .NET runtime (no installation needed) |
| `-Trimmed` | Switch | Removes unused assemblies (smaller size) |

## Recommended Build Commands

### Development/Testing

```powershell
# Quick debug build
.\Build-Executable.ps1 -Configuration Debug

# Or just run without building executable
.\run-app.ps1
```

### Production Deployment

```powershell
# Recommended production build
.\Build-Executable.ps1 -Configuration Release -SingleFile -SelfContained

# Smaller production build (with trimming)
.\Build-Executable.ps1 -Configuration Release -SingleFile -SelfContained -Trimmed
```

### CI/CD Pipeline

```powershell
# Automated build with verification
.\Build-Executable.ps1 -Configuration Release -SingleFile -SelfContained
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build succeeded, ready for deployment"
}
```

## Verifying Build Configuration

After building, you can verify which configuration was used:

1. **Check the build log** - shows Configuration parameter
2. **File location** - `publish\win-x64-Release` or `publish\win-x64-Debug`
3. **Run the app** - Debug builds show additional logging
4. **Check default paths** - Open Settings window to see default directories

## Troubleshooting

**Q: Changes to paths not taking effect?**

- Ensure you're building with the correct Configuration (`-Configuration Release`)
- Delete the `bin` and `obj` folders and rebuild
- Check the Settings database hasn't saved different values

**Q: Need different paths for testing?**

- Build with `-Configuration Debug`
- Or change the DEBUG paths in AppSettings.cs

**Q: Want to add a new environment (e.g., UAT)?**

- See "Option 1: Add Custom Preprocessor Symbols" above
- Create custom configuration in .csproj file
