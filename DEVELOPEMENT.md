# Data Transfer Application - .NET 8 WPF

A modern .NET 8 WPF application for secure data transfer between air-gapped systems with comprehensive audit trails and compliance controls.

## 🚀 Project Status

**Current Progress:**
- ✅ Project scaffolding (.NET 8 WPF)
- ✅ NuGet packages installed
- ✅ Project structure created
- ✅ Core models implemented
- ✅ LoggingService implemented
- ✅ SettingsService implemented
- ⏳ Remaining services (in progress)
- ⏳ ViewModels (pending)
- ⏳ Views/XAML (pending)

## 📦 Installed Packages

- **SharpCompress** (0.44.0) - Archive handling (zip, rar, 7z, tar, gz, etc.)
- **LiteDB** (5.0.21) - Embedded NoSQL database for settings and transfer history
- **Serilog** (4.3.0) - Structured logging framework
- **Serilog.Sinks.File** (7.0.0) - File logging sink
- **CommunityToolkit.Mvvm** (8.4.0) - MVVM helpers and source generators
- **EPPlus** (7.7.0) - Excel file generation for compliance records

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
│   ├── AuditService.cs         ⏳ To create
│   ├── TransferService.cs      ⏳ To create
│   └── ArchiveService.cs       ⏳ To create
├── ViewModels/
│   ├── MainViewModel.cs        ⏳ To create
│   └── SettingsViewModel.cs    ⏳ To create
├── Views/
│   ├── MainWindow.xaml         ⏳ To update
│   └── SettingsWindow.xaml     ⏳ To create
├── Helpers/
│   └── RelayCommand.cs         ⏳ To create
└── App.xaml                    ⏳ To update
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

- **SharpCompress** (0.44.0) - Archive handling
- **LiteDB** (5.0.21) - Embedded database
- **Serilog** (4.3.0) - Structured logging
- **CommunityToolkit.Mvvm** (8.4.0) - MVVM helpers
- **EPPlus** (7.7.0) - Excel generation

## �🎯 Key Features

### Completed
- ✅ LiteDB settings backend with automatic creation in AppData
- ✅ Serilog file logging with configurable levels
- ✅ Comprehensive data models for folders, files, audits, and transfers
- ✅ Settings management with defaults

### To Implement
- ⏳ Archive handling with SharpCompress
- ⏳ Folder auditing with regex validation
- ⏳ File transfer operations
- ⏳ MVVM pattern with data binding
- ⏳ Modern WPF UI matching original design
- ⏳ Settings window

## 📝 Next Steps

### 1. Create Remaining Services

Create these files in `Services/`:

**AuditService.cs** - Folder and file validation
- Folder naming validation using regex
- File extension blacklist checking
- Dataset whitelist validation
- Generate comprehensive audit results

**TransferService.cs** - File transfer operations
- Copy folders to destination drives
- Calculate file hashes (optional)
- Progress reporting
- Error handling and rollback

**ArchiveService.cs** - Archive file handling
- List archive contents using SharpCompress
- Support for zip, rar, 7z, tar, gz, bz2, xz
- Extract archive information
- Preview file listings

### 2. Create ViewModels

**MainViewModel.cs** - Main window logic
- ObservableCollection for folders and files
- Commands for Refresh, Audit, Transfer operations
- Progress reporting
- Status updates

**SettingsViewModel.cs** - Settings window logic
- Bind to AppSettings model
- Save/Cancel/Reset commands
- Validation logic

### 3. Update Views

**MainWindow.xaml** - Port from PowerShell design
- Elegant tabbed interface
- Folder list with status indicators
- File DataGrid with view buttons
- Transfer progress bar
- Statistics panel

**SettingsWindow.xaml** - Configuration UI
- Grouped settings (Paths, Audit, Logging, Transfer)
- Input validation
- Save/Cancel/Reset buttons

### 4. Update App.xaml.cs

Initialize services at startup:
```csharp
// Get AppData path
var appDataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "DataTransferApp");

// Initialize services
var dbPath = Path.Combine(appDataPath, "settings.db");
var logPath = Path.Combine(appDataPath, "Logs", "app.log");

var settingsService = new SettingsService(dbPath);
var settings = settingsService.GetSettings();

var logLevel = LoggingService.ParseLogLevel(settings.LogLevel);
LoggingService.Initialize(logPath, logLevel);
```

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

**Version**: 2.0.0  
**Last Updated**: January 27, 2026  
**Framework**: .NET 8.0  
**UI**: WPF with MVVM
