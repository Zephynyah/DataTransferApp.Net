# Changelog

All notable changes to Data Transfer Application (.NET) will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.1] - 2026-02-12

### Added

- Enhance SettingsWindow: add callback for settings saved notification and clean up ComboBox bindings
- Add CloseSettingsOnSave option and update settings save behavior
- Add XML documentation for filePath parameter in IsAsciiFileDotNet6 method
- Add XML documentation for filePath parameter in IsTextFile method
- Add application icon for DataTransferApp
- Add XML documentation for GetTransferByIdAsync method parameter
- Add viewable file extensions to AppConstants; clean up unused usings in services
- Add HashAlgorithm and ConflictResolution options to AppSettings; update SettingsWindow for improved compliance record handling
- Add IconToVisibilityConverter and update SettingsItem to support icons in the UI
- Add AwesomeLabelTooltip control and update SettingsWindow for improved tooltip integration

### Fixed

- Enhance logging in RoboSharpProgressAdapter for better debugging; adjust UI layout in MainWindow for improved visibility
- Fixed settings save notification appearing only after settings window closes instead of immediately when saved

### Changed

- Moved application icons/images to Assets directory
- Enhance settings saving logic: allow settings reload on save confirmation or dialog closure
- Refactor constants in services and helpers: replace hardcoded values with AppConstants for improved maintainability and consistency
- Update snackbar properties: adjust background color and opacity for improved visibility
- Refactor MainViewModel and ViewModelBase: remove unused properties and enhance snackbar functionality with error handling and loading state management
- Remove unused using directives in ChangesViewModel and TransferHistoryViewModel
- Refactor comments for clarity in RoboSharpTransferEngine and TransferProgress classes
- Remove ShowFolderAuditDetailsIcon property and related bindings from MainViewModel for cleaner code
- Remove ShowFolderAuditDetailsIcon property and related UI elements from SettingsWindow for cleaner interface
- Refactor UI components and remove unused controls; update SettingsWindow for improved layout and functionality
- Update RetentionDays binding in SettingsWindow and ensure property change notification in AppSettings

## [1.4.0] - 2026-02-09

### Added

- Added RoboSharpTransferEngine to hande file transfers with robust features like progress reporting, cancellation support, and retry logic
  - Exponential backoff retry logic (1s → 2s → 4s)
  - Jitter implementation (±25% randomness) to prevent retry storms
  - Cancellation handling during retry operations
  - Success and failure scenarios
  - Callback invocations for retry events
  - Argument validation and edge case handling

### Changed

- Test project structure enhanced with 40 comprehensive tests
- Tests use temporary GUID-based directories for isolation
- IDisposable pattern implemented for automatic cleanup

### Fixed

- Fixed UseRoboSharp checkbox not updating UI on toggle
- Fixed missing RoboSharpTransferEngine tests and added comprehensive coverage
- Fixed missing RetryHelper tests and added comprehensive coverage

## [1.3.5] - 2026-02-02

### Added

- Add E:\ and T:\ to excluded drives
- Add command for transferring all folders
- Add conditional compilation for TransferHistoryDatabasePath
- Refactor LoggingService to use Shutdown method;  
- Add missing newlines in SettingsService and TransferHistoryService
- Introduce timeUpdateTimer in MainViewModel for improved functionality

### Fixed

- Update CHANGELOG with fixed issues

### Changed

- Update SizeFormatted and CanTransfer properties in FileData and FolderData models
- Refactor code style and improve logging in TransferService methods
- Remove IsEnabled binding from Transfer All button
- Implement subdirectory copying and file retry logic
- Create Build Configuration Guide
- Refactor AppSettings for directory paths and regex
- Enhance retention folder deletion with retries
- Enhance logging for database file lock handling

## [1.3.4] - 2026-02-01

### Added

- Enhance AppSettings validation and UI
- Add unit tests for FileService and update solution configuration; suppress StyleCop warnings
- Refactor various services for improved readability and maintainability; add missing newlines and reorganize code structure
- Refactor TransferDatabaseService and TransferHistoryService; add InitializeDatabase method for index management and clean up code structure
- Refactor LoggingService to use Shutdown method; update SizeFormatted and CanTransfer properties in FileData and FolderData models; add missing newlines in SettingsService and TransferHistoryService
- Introduce timeUpdateTimer in MainViewModel for improved functionality
- Refactor application startup logic for improved readability and maintainability; add new models for transfer and audit processes; remove unused classes and consolidate functionality.
- Add DatasetValidation, ExtensionValidation, and FileViolation models for dataset and file validation
- Refactor compliance record service documentation for clarity; update return type descriptions; enhance check-quality scripts for improved analysis and error handling; add batch script for code quality checks
- Update DEVELOPEMENT.md for formatting consistency; enhance AppSettings with additional dataset in whitelist; improve dataset validation logic in AuditService; refine check-quality script documentation
- Refactor MainWindow layout and UI elements for improved consistency; add changelog file and update check-quality script with detailed documentation. Enhance application versioning in package.json and update CHANGELOG.md with recent changes and fixes.
- Retry logic for locked/in-use files during transfer operations (3 attempts with 500ms delay)
- Automatic read-only attribute removal for source and destination files during transfer
- Complete subdirectory structure preservation including empty folders
- Conditional compilation directives for Debug vs Release builds with environment-specific default paths
- BUILD-CONFIGURATION.md documentation for build-time configuration options
- CanExecute validation for Transfer All command to ensure drive is selected before enabling

### Changed

- Remove trailing newlines in SettingsService and TransferHistoryService for code cleanliness
- Refactor TransferDatabaseService and MainViewModel
- Refactor TransferDatabaseService and MainViewModel for improved structure; move DriveAction enum in MainViewModel, and ensure proper initialization in TransferDatabaseService
- Refactor various services for improved resource management, error handling, and regex performance; enhance boolean conversion logic and wildcard pattern matching
- Refactor AuditService and TransferService for improved readability and maintainability
- Refactor various services and helpers for improved error handling, performance, and code clarity; update logging and snackbar methods for consistency
- Refactor FileService and enhance file handling capabilities
- Database locking warnings reduced to debug level during retry attempts (only warning on final failure)
- Retention cleanup error handling improved with retry logic and better UNC network share support
- Default directory paths now use conditional compilation (local paths for Debug, network UNC paths for Release)

### Fixed

- Empty nested folders not being transferred (subfolder now preserved)
- Access denied errors when deleting retention folders on UNC network shares
- Read-only attribute conflicts preventing file transfer and folder cleanup
- Locked files causing entire transfer to fail instead of retrying
- Excessive warning log spam during normal database retry operations
- Transfer All button now properly disabled when no drive is selected or detected

## [1.3.3] - 2026-01-30

### Added

- Changelog window with markdown viewer for displaying change history
- Splash screen with loading indication
- Help window for application documentation
- FontAwesome icons support across UI elements
- Animation support with Loaded event in MainWindow
- Moved CHANGELOG.md to Resources directory for better organization
- Updated project file to reference local CHANGELOG.md
- Background retention cleanup functionality with UI status indicators
- PluralizationConverter for proper singular/plural text display (e.g., "1 Day" vs "7 Days")
- InverseBooleanToVisibilityConverter for improved UI binding logic
- Debug logging for retention cleanup operations and simulation

### Changed

- Refactored ViewModels to inherit from ViewModelBase for consistency
- Enhanced UI styles and layouts in various windows (HelpWindow, SettingsWindow, etc.)
- Improved icon usage with FontAwesome instead of Path elements
- Enhanced ChangesWindow with improved MarkdownViewer styling and margins
- Better FlowDocument styling in ChangesWindow for improved layout
- Updated retention cleanup UI with animated status indicators and proper pluralization

### Fixed

- HelpWindow icon path for correct display
- File viewability check in FileService
- Various UI alignment and styling issues
- Retention cleanup status display and animation timing

## [1.3.2] - 2026-01-25

### Added

- Transfer history database with significant performance improvements (50-200x)
- Automated compliance records generation (CSV/Excel)
- Multi-user support with shared database
- Migration tools for importing existing transfer logs
- Archive file support with preview and validation
- Real-time transfer progress statistics
- Automatic settings persistence
- Enhanced file detection with special handling for .md files
- Better detection of viewable files using content analysis
- PDF export option for compliance reports
- Enhanced progress bar animations and status indicators

### Changed

- Database backend for transfer history storage
- Improved concurrent user handling
- Enhanced folder naming and file type validation
- Progress reporting system
- Compliance record settings and generation
- Transfer logging to transfer records

### Fixed

- Memory leaks during bulk file operations
- Issues with file hash calculation for large files
- Path validation to prevent directory traversal attacks

### Security

- Improved path validation

### Performance

- Optimized memory usage during bulk file operations
- Improved file detection algorithms
- Sample-based file reading for faster detection
- Improved loading times for large directories

## [1.3.0] - 2025-09-XX

### Added

- Compliance Source Location setting
- Compliance Record Type selection with Standard record generation
- Wildcard matcher for excluded folders
- Folder exclusion feature with UI management
- Retention period setting and folder retention cleanup
- Settings for audit summary display and window startup mode
- Auto-audit on startup option
- Show folder audit details icon setting
- Enhanced ListBox item template with audit status indicators
- Folder info display with size and file count
- New icon geometries for checklist, block, and package
- Application icon (app.ico and app.png)
- Command notifications for folder and drive selection changes

### Changed

- Refactored file and folder models to use observable properties
- Enhanced audit status converters and UI bindings
- Updated StatusToColorConverter colors
- Improved MainWindow layout and button styles
- Enhanced transfer logging settings
- Added conflict handling settings and drive content checks
- Implemented transfer history feature with view model, service, and UI components

### Fixed

- Validation messages for consistency
- UI element properties and margins
- DataGrid RowHeight for better readability
- Archive handling for compound archives
- Error handling in TransferHistoryService

## [1.2.0] - 2025-08-XX

### Added

- Initial release of Data Transfer Application (.NET)
- Air-gapped data transfer functionality
- Folder auditing and validation
- Compliance record generation
- Transfer history tracking
- Configurable settings and validation rules
- WPF user interface with MVVM architecture
- Build script for publishing as standalone executable
- Application manifest and icon
- Input dialog for user inputs
- Audit status handling with detailed failure messages

### Security

- Path validation to prevent unauthorized access
- File type restrictions and dataset authorization
- Audit trail generation for all transfers

### Changed

- Target framework and package versions
- Application settings paths
- Logging messages and UI styling

## [1.1.0] - 2025-07-XX

### Added

- Enhanced archive handling with Reader approach
- Improved extraction logic for various archive types
- Additional UI controls and styling enhancements

### Changed

- Refactored styles in MainWindow and App.xaml
- Added ApplicationGeometry and adjusted margins

## [1.0.0] - 2025-06-XX

### Added

- Basic project structure and initial commit
- Core functionality setup
