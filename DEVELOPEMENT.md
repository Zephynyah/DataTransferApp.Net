# Data Transfer Application - .NET 8 WPF

A modern .NET 8 WPF application for secure data transfer between air-gapped systems with comprehensive audit trails and compliance controls.

## 🚀 Project Status

**Current Progress:**
- ✅ Project scaffolding (.NET 8 WPF)
- ✅ NuGet packages installed
- ✅ Project structure created
- ✅ Core models implemented
- ✅ All services implemented
- ✅ ViewModels implemented
- ✅ Views/XAML implemented
- ✅ Helpers implemented
- ✅ App.xaml updated
- ✅ Application builds successfully

## 📦 Installed Packages

- **SharpCompress** (0.44.2) - Archive handling (zip, rar, 7z, tar, gz, etc.)
- **LiteDB** (5.0.21) - Embedded NoSQL database for settings and transfer history
- **Serilog** (4.3.0) - Structured logging framework
- **Serilog.Sinks.File** (7.0.0) - File logging sink
- **CommunityToolkit.Mvvm** (8.4.0) - MVVM helpers and source generators
- **EPPlus** (8.4.1) - Excel file generation for compliance records

## 🏗️ Project Structure

```
DataTransferApp.Net/
├── Models/
│   ├── AppSettings.cs          ✅ Complete
│   ├── FileData.cs             ✅ Complete
│   ├── FolderData.cs           ✅ Complete
│   ├── AuditResult.cs          ✅ Complete
│   └── TransferLog.cs          ✅ Complete
├── Services/
│   ├── LoggingService.cs       ✅ Complete
│   ├── SettingsService.cs      ✅ Complete
│   ├── AuditService.cs         ✅ Complete
│   ├── TransferService.cs      ✅ Complete
│   ├── ArchiveService.cs       ✅ Complete
│   ├── ComplianceRecordService.cs ✅ Complete
│   ├── FileService.cs          ✅ Complete
│   ├── TransferDatabaseService.cs ✅ Complete
│   └── TransferHistoryService.cs ✅ Complete
├── ViewModels/
│   ├── MainViewModel.cs        ✅ Complete
│   └── TransferHistoryViewModel.cs ✅ Complete
├── Views/
│   ├── MainWindow.xaml         ✅ Complete
│   ├── SettingsWindow.xaml     ✅ Complete
│   ├── TransferHistoryWindow.xaml ✅ Complete
│   ├── AboutViewWindow.xaml    ✅ Complete
│   ├── ArchiveViewerWindow.xaml ✅ Complete
│   ├── FileViewerWindow.xaml   ✅ Complete
│   └── ProgressWindow.xaml     ✅ Complete
├── Helpers/
│   ├── RelayCommand.cs         ✅ Complete
│   ├── FileEncodingHelper.cs   ✅ Complete
│   ├── VersionHelper.cs        ✅ Complete
│   ├── AuditStatusToBrushConverter.cs ✅ Complete
│   ├── FileIconConverter.cs    ✅ Complete
│   ├── FileRowBackgroundConverter.cs ✅ Complete
│   ├── FileSizeConverter.cs    ✅ Complete
│   ├── InverseBooleanConverter.cs ✅ Complete
│   ├── ListToStringConverter.cs ✅ Complete
│   ├── NullToVisibilityConverter.cs ✅ Complete
│   ├── StatusToColorConverter.cs ✅ Complete
│   └── WildcardMatcher.cs      ✅ Complete
└── App.xaml                    ✅ Complete
```

## �️ Architecture

- **Framework**: .NET 8.0 WPF with MVVM pattern
- **Database**: LiteDB for settings and transfer history
- **Logging**: Serilog with file rotation
- **UI**: Modern WPF with data binding
- **Packaging**: SharpCompress for archive handling

## 🛠️ Development Setup

```bash
# Clone repository
git clone <repository-url>
cd DataTransferApp.Net

# Build project
dotnet build

# Run application
dotnet run
```

## 🔧 Key Components

- **Models**: Data structures for files, folders, transfers, and settings
- **Services**: Business logic for auditing, transfers, and archiving
- **ViewModels**: MVVM logic with observable properties and commands
- **Views**: XAML UI components
- **Helpers**: Utility classes and converters

## 📦 Dependencies

- **SharpCompress** (0.44.2) - Archive handling
- **LiteDB** (5.0.21) - Embedded database
- **Serilog** (4.3.0) - Structured logging
- **CommunityToolkit.Mvvm** (8.4.0) - MVVM helpers
- **EPPlus** (8.4.1) - Excel generation

## �🎯 Key Features

### Completed
- ✅ LiteDB settings backend with automatic creation in AppData
- ✅ Serilog file logging with configurable levels
- ✅ Comprehensive data models for folders, files, audits, and transfers
- ✅ Settings management with defaults
- ✅ Archive handling with SharpCompress (zip, rar, 7z, tar, gz, etc.)
- ✅ Folder auditing with regex validation
- ✅ File transfer operations with progress reporting
- ✅ MVVM pattern with data binding and CommunityToolkit.Mvvm
- ✅ Modern WPF UI with comprehensive views and windows
- ✅ Transfer history database with LiteDB
- ✅ Automated compliance record generation with EPPlus
- ✅ File viewer and archive viewer windows
- ✅ Comprehensive helper utilities and converters
- ✅ Application builds and runs successfully

## 📝 Next Steps

### 1. Testing & Validation
- ✅ Application builds successfully
- ⏳ Run comprehensive testing of all features
- ⏳ Test file transfer operations with various file types
- ⏳ Test archive handling with different formats
- ⏳ Validate audit functionality with various folder structures
- ⏳ Test settings persistence and UI responsiveness

### 2. Documentation Updates
- ⏳ Update README.md with complete feature list
- ⏳ Create user manual for end users
- ⏳ Document configuration options and best practices
- ⏳ Add troubleshooting guide

### 3. Potential Enhancements
- ⏳ Add unit tests for services
- ⏳ Implement automated build pipeline
- ⏳ Add more archive format support if needed
- ⏳ Enhance error reporting and user feedback
- ⏳ Add export functionality for transfer history

## 🔧 Configuration

Settings are stored in: `%AppData%\DataTransferApp\settings.db`  
Logs are stored in: `%AppData%\DataTransferApp\Logs\`

### Configurable Settings

- **Paths**: Staging, Retention, Logs directories
- **Folder Naming**: Regex pattern for validation
- **File Extensions**: Blacklist for prohibited file types
- **Datasets**: Whitelist for allowed dataset codes
- **Logging**: Level (Debug/Info/Warning/Error), format, rotation
- **Transfer**: Hash calculation, compression, concurrency
- **UI**: Window size, notifications

## 🏃 Running the Application

```powershell
cd DataTransferApp.Net
dotnet build
dotnet run
```

## 📚 Development Guidelines

### MVVM Pattern
- Use `CommunityToolkit.Mvvm` for `ObservableProperty` and `RelayCommand`
- ViewModels should not reference Views directly
- Use data binding for all UI updates

### Logging
- Use `LoggingService` for all logging
- Log levels: Debug, Info, Warning, Error, Success
- Include context in log messages

### Async/Await
- Use async methods for I/O operations
- Report progress for long-running operations
- Handle cancellation tokens

### Error Handling
- Use try-catch blocks appropriately
- Log exceptions with context
- Show user-friendly error messages

## 🔒 Security Considerations

- No network operations (air-gapped design)
- File path validation to prevent traversal attacks
- Settings stored locally in LiteDB
- Audit trail for all transfers
- Configurable file extension blacklist

## 📖 Migration from PowerShell

This .NET application improves upon the PowerShell version:
- ✅ Better performance with native compiled code
- ✅ Robust archive handling with SharpCompress
- ✅ Persistent settings with LiteDB
- ✅ Professional logging with Serilog
- ✅ Modern MVVM architecture
- ✅ Better error handling and validation
- ✅ Configurable log levels and rotation
- ✅ **LiteDB database for centralized transfer history (v1.2.0)**
- ✅ **Automated compliance record generation (v1.2.0)**

## 📚 Development Guidelines

### MVVM Pattern
- Use `CommunityToolkit.Mvvm` for `ObservableProperty` and `RelayCommand`
- ViewModels should not reference Views directly
- Use data binding for all UI updates

### Logging
- Use `LoggingService` for all logging
- Log levels: Debug, Info, Warning, Error, Success
- Include context in log messages

### Async/Await
- Use async methods for I/O operations
- Report progress for long-running operations
- Handle cancellation tokens

### Error Handling
- Use try-catch blocks appropriately
- Log exceptions with context
- Show user-friendly error messages

## 🔒 Security Considerations

- Air-gapped design with no network operations
- File path validation to prevent traversal attacks
- Settings stored locally in LiteDB
- Audit trail for all transfers
- Configurable file extension blacklist

## 🤝 Contributing

When extending this application:
1. Follow existing MVVM patterns and conventions
2. Add comprehensive logging for all operations
3. Write unit tests for new services
4. Update documentation for user-facing changes
5. Test on multiple Windows versions

## 📄 License

Internal use only - Air-gapped transfer system

---

**Version**: 1.3.0  
**Last Updated**: January 28, 2025  
**Framework**: .NET 8.0  
**UI**: WPF with MVVM
