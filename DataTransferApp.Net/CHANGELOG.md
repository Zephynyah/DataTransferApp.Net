# Changelog

All notable changes to this project will be documented in this file.

## [1.4.1] - 2026-02-11

### Added

- Add Packaged parameter to create zip archive
- Enhance Snackbar notification: adjust background colors for improved visibility, add opacity control, and implement slide-in/out animations for a smoother user experience.
- Update changelog for version 1.4.0: add comprehensive unit test suites for RoboSharpTransferEngine and RetryHelper, enhance test project structure, and document testing insights. Update TODO to reflect completed unit testing tasks.
- Refactor error tooltip handling: remove custom error message logic from FileErrorTooltipConverter and streamline archive file checks in ArchiveService. Update MainViewModel to use Array.Exists for extension checks. Improve code readability by removing unnecessary lines and adding spacing in RoboSharpProgressAdapter. Update TODO for tested cancel functionality and add comprehensive tests for RoboSharpTransferEngine.
- Enhance logging functionality: implement log cleanup for old RoboSharp transfer logs and add retention days constant
- Remove AwesomeDialog control and associated files; add Ookii.Dialogs.Wpf package for enhanced dialog functionality
- Add ModernToolTip control for enhanced tooltip functionality; update MainWindow and AwesomeToolTip for improved user guidance
- Add ThisMonthTransfers property and update TransferHistoryWindow to display monthly transfer count
- Enhance tooltips with detailed file information and error handling; add FileListToolTip control for improved user guidance on file status and actions
- Enhance AwesomeToolTip with dynamic error messages and details; update MainViewModel to include additional recommended actions for file transfer
- Add FileErrorTooltipConverter and enhance AwesomeToolTip with new properties for file status
- Refactor XAML resources and add AwesomeToolTip control; enhance UI with custom tooltips and improve application startup script
- Implement cancel functionality for active transfers; add CancelTransfer command and UI button
- Mark progress tracking tasks as complete; verify real-time updates, add speed calculation validation, and test ETA accuracy
- Add detailed error tooltips for files; implement FileErrorTooltipConverter and update FileData model
- Refactor UI layout for statistics display; adjust margins and padding for improved aesthetics
- Add "Cancel Transfer" menu item and remove visible cancel button; enhance UI for transfer management
- Enhance progress tracking and cancellation features; improve UI responsiveness and logging; update settings for better user experience
- Enhance progress tracking and cancellation in transfer operations; implement accurate totals from list-only scans, improve UI responsiveness, and add transfer completion status
- Filter out hidden and system files in directory checks; add helper method for attribute verification
- Add pre-scan for directory totals to improve ETA accuracy during transfers
- Add transfer status indicators and update UI for active transfers
- Add RoboSharp preset configuration and binding converters for enhanced settings management
- Add configuration presets for RoboSharp settings and update TODO
- Add toggle for list and table views in Transfer History with corresponding bindings
- Add Robocopy verbose output option and update bindings in settings
- Add RoboSharp file transfer functionality with detailed options and results

### Fixed

- Enhance logging in RoboSharpProgressAdapter for better debugging; adjust UI layout in MainWindow for improved visibility

### Changed

- Enhance folder opening logic with error handling and staging directory validation
- minor changes
- Refactor application structure and enhance documentation
- Refactor application structure and enhance documentation
- Update CHANGELOG.md
- Refactor UseRoboSharp and UseMultithreadedCopy properties
- Introduce transfer engine property and update logic
- Update Transfer Engine Status display in XAML
- Update CHANGELOG for version 1.4.0
- Implement cleanup for remaining items after transfer
- Update transfer engine name from 'RoboSharp' to 'Robocopy'
- Refactor validation methods for folder name format
- Update progress messages and adjust layout in MainWindow: change ETA display to indicate folder movement and expand TextBlock column span for retention days.
- Enhance audit failure dialog: adjust width of the override audit failure dialog for improved visibility
- Refactor file size formatting: centralize file size formatting logic into FileSizeHelper class and update references throughout the codebase
- Refactor retention folder handling: ensure consistent folder naming for retention and destination, and improve conflict resolution logic
- Refactor drive clearing logic: streamline confirmation handling and separate internal clear drive method
- Refactor drive clearing confirmation: suppress confirmation dialog when already confirmed by user and update drive clear button label to include drive letter
- Refactor drive content handling: streamline button descriptions and replace fallback MessageBox with logging for unsupported operating systems
- Refactor archive handling: centralize compressed file extensions in AppConstants and streamline archive checks in ArchiveService and MainViewModel
- Refactor AwesomeDialog constructor: clean up whitespace and improve Dispatcher invocation for keyboard focus
- Implement AwesomeDialog and AwesomeToolTip controls; remove ModernToolTip and update MainWindow for enhanced user interaction
- Remove AwesomeToolTip control and associated code files to streamline tooltip functionality
- Update ModernToolTip control: adjust icon font size and clean up unused example code
- Update TransferHistoryViewModel to show table view by default; enhance MainWindow and TransferHistoryWindow XAML for improved tooltip handling and layout adjustments
- Enhance AwesomeToolTip with detailed file information, error messages, and recommended actions; update bindings in MainWindow.xaml for improved user guidance
- Enhance AwesomeToolTip with file details and warnings; update converters in App.xaml and clean up unused resources in various views
- Update error tooltips and messages for compressed files; enhance user guidance on file inspection and integrity verification
- Prevent race condition in transfer status update; ensure transfer activation only occurs if not already active and not a completion update
- Remove cancellation functionality from transfer process; update UI to reflect changes
- Merge branch 'main' of <https://github.com/Zephynyah/DataTransferApp.Net>
- Refactor transfer button logic and UI; enhance visibility handling for transfer and cancel actions
- Reduce icon font size in SettingsWindow for improved UI consistency
- Enhance progress estimation handling in RoboSharpProgressAdapter; update total files and bytes only with valid estimates, and improve logging for better diagnostics
- Enhance ETA display logic in MainViewModel; improve speed calculation and logging in RoboSharpProgressAdapter; update log file path creation in TransferService
- Enhance ETA calculations and display in progress updates; improve speed calculation logic for better accuracy
- Refactor RoboSharp transfer handling; introduce event arguments for error and transfer results, streamline transfer logic, and enhance logging
- Refactor error handling and improve logging functionality; update collections to use IList for better abstraction
- Refactor and enhance code documentation across multiple files
- Reduce minimum width of transfer engine status card for improved layout
- Enhance transfer engine status UI with animated icons and improve layout for better visibility
- Refactor logging path generation and enhance transfer engine status UI with animated icons
- Implement exponential backoff retry logic and update configuration presets
- Replace RoboSharp statistics border with expander for improved UI and organization
- Implement confirmation dialog on application close with cleanup handling
- Enhance documentation for large file copy strategies in TODO.md
- Document large file copy strategies in TODO.md
- Update icon for 'Run Retention Cleanup' menu item in MainWindow.xaml
- Update version to 1.3.5 and enhance CHANGELOG with recent changes
- Merge branch 'main' of <https://github.com/Zephynyah/DataTransferApp.Net>

## [1.4.0] - 2026-02-08

### Added

- Comprehensive unit test suite for RoboSharpTransferEngine (24 integration tests)
  - Transfer folder tests with subdirectories and recursion
  - Transfer specific files with path structure preservation
  - Transfer estimation and size calculation
  - Progress reporting and event handling
  - Cancellation token support
  - File and directory exclusion filters
  - Large file transfers (10MB+) and bulk file operations (100+ files)
  - Result validation and statistics verification
- Comprehensive unit test suite for RetryHelper (16 unit tests)
  - Exponential backoff retry logic (1s → 2s → 4s)
  - Jitter implementation (±25% randomness) to prevent retry storms
  - Cancellation handling during retry operations
  - Success and failure scenarios
  - Callback invocations for retry events
  - Argument validation and edge case handling
- Unit testing documentation (UNIT_TESTS_SUMMARY.md)
  - Test coverage breakdown
  - Test design patterns
  - Key insights and RoboCopy exit code behavior
  - Future testing priorities

### Changed

- Test project structure enhanced with 40 comprehensive tests
- All tests passing (100% success rate)
- Tests use temporary GUID-based directories for isolation
- IDisposable pattern implemented for automatic cleanup
- Updated TODO.md to mark unit testing tasks as complete

### Technical Details

- Test execution time: ~11.2 seconds
- Integration tests use real file system operations with RoboSharp
- RetryHelper tests validate exponential backoff algorithm
- Tests cover async operations, cancellation, progress reporting, and error scenarios

## [1.3.5] - 2026-02-02

### Added

- Update project version to 1.3.4 and add WinDefender utility class for virus scanning
- Add E:\ and T:\ to excluded drives
- Add command for transferring all folders
- Add conditional compilation for TransferHistoryDatabasePath
- Refactor LoggingService to use Shutdown method; update SizeFormatted and CanTransfer properties in FileData and FolderData models; add missing newlines in SettingsService and TransferHistoryService; introduce timeUpdateTimer in MainViewModel for improved functionality

### Fixed

- Update CHANGELOG with fixed issues

### Changed

- Refactor code style and improve logging in TransferService methods
- Remove WinDefender utility class for virus scanning
- Update CHANGELOG for version 1.3.4 enhancements
- Remove IsEnabled binding from Transfer All button
- Update CHANGELOG for version 1.3.5
- Implement subdirectory copying and file retry logic
- Update CHANGELOG for version 1.3.4
- Create Build Configuration Guide
- Refactor AppSettings for directory paths and regex
- Enhance retention folder deletion with retries
- Enhance logging for database file lock handling
- Refactor converters and improve documentation; remove unused FileToFontAwesomeConverter
- Update CHANGELOG.md to reflect recent enhancements and refactoring; improve clarity and organization
- Merge branch 'main' of <https://github.com/Zephynyah/DataTransferApp.Net>

## [1.3.5] - 2026-02-01

### Added

- Enhance AppSettings validation and UI; update version to 1.3.5
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

### Changed

- Remove trailing newlines in SettingsService and TransferHistoryService for code cleanliness
- Refactor TransferDatabaseService and MainViewModel
- Refactor TransferDatabaseService and MainViewModel for improved structure; move DriveAction enum in MainViewModel, and ensure proper initialization in TransferDatabaseService
- Refactor various services for improved resource management, error handling, and regex performance; enhance boolean conversion logic and wildcard pattern matching
- Refactor AuditService and TransferService for improved readability and maintainability
- Refactor various services and helpers for improved error handling, performance, and code clarity; update logging and snackbar methods for consistency
- Refactor FileService and enhance file handling capabilities

## [1.3.4] - 2026-01-31

### Added

- Implement feature X to enhance user experience and optimize performance
- Refactor code for improved readability and consistency across services and view models; added code quality check script.
- Add PluralizationConverter and enhance UI for retention cleanup status in MainWindow; update retention message in SettingsWindow
- Add debug logging for retention cleanup simulation in TransferService and enhance MainWindow UI with icons for retention status
- Add comments to clarify FlowDocument margin settings in ChangesWindow
- Refactor ChangesViewModel and ChangesWindow: remove unused PropertyChanged event, adjust FlowDocument styles, and add MarkdownViewer styling for improved layout
- Refactor changelog management: remove old CHANGES.md, add CHANGELOG.md with detailed version history, and implement ChangesViewModel for markdown rendering in ChangesWindow.
- Add changelog file and remove unused scroll event handler from ChangesWindow
- Add ScrollViewer scroll change event handler for optional smooth scrolling
- Implement changelog feature with dedicated window and markdown viewer; update .gitignore and project dependencies
- Add splash screen implementation with timer for loading indication
- Enhance HelpWindow and SettingsWindow UI: restructure layout, add icons, and improve header styles for better user experience
- Add AnimationBlueBrush to App.xaml and implement Loaded event in MainWindow for animation support
- Add HelpWindow XAML and code-behind for application help documentation
- Add FontAwesome icons support and update UI styles across various windows
- Enhance transfer management: add async delete functionality, improve database health checks, and update UI with database status
- Add Compliance Source Location setting and update related services
- Add Compliance Record Type selection and implement Standard record generation
- Add additional rows for Standard, Summary, and Combined in simple.txt
- Add Edit functionality for excluded folders in SettingsWindow
- Add FootnoteStyle to SettingsWindow.xaml
- Add wildcard matcher for excluded folders
- Add WildcardMatcher class for pattern matching
- Add header information to simple.txt
- Add exclude drives input and description in settings
- Add application footer to MainWindow and implement auto-selection of first folder in MainViewModel
- Update ComboBox and ContextMenu styles for improved alignment and padding
- Add application manifest file and update project file to include it
- Add ApplicationIcon property to project file
- Add caution folder tracking and update UI to display caution status in MainWindow
- Add new converters for audit status icons; update UI elements and improve layout in TransferHistoryWindow and MainWindow
- Add compliance record generation and transfer history database support; implement TransferDatabaseService and ComplianceRecordService for enhanced data management
- Add Trimmed option for executable build
- Add application version to MainViewModel; enhance AboutViewWindow layout and content
- Add About window and command; update StatusToColorConverter colors; enhance MainWindow context menu
- Add NullToVisibilityConverter and update UI bindings; modify application settings and icon geometries
- Add build script for publishing Data Transfer Application as a standalone executable
- Add user information and application details to MainWindow; enhance InputDialog styling; update .gitignore to exclude scripts and docs
- Enhance audit status handling by adding dataset audit properties and combining failure reasons; update UI to display detailed failure messages
- Refactor styles in MainWindow and App.xaml for improved consistency; add ApplicationGeometry and adjust margins for better layout
- Add scripts directory to .gitignore to exclude script files from version control
- Add folder exclusion feature; implement UI for managing excluded folders and create input dialog for folder names
- Add retention period setting and implement folder retention cleanup; enhance settings UI with new controls
- Add settings for audit summary display and window startup mode; enhance UI with new controls
- Add ShowFolderAuditDetailsIcon setting and UI controls for folder audit details display
- Add AutoAuditOnStartup setting and UI controls for startup behavior
- Enhance ListBox item template with improved styling and audit status indicators; add folder info display with size and file count
- Add new icon geometries for checklist, block, and package; update MainWindow bindings to use new converters for audit status display
- Update StatusToColorConverter to use black for unknown status and modify MainWindow to bind Foreground property; add ValueToColorConverter for numeric status representation
- Add application icon image to DataTransferApp.Net
- Add application icon image (app.png) to the project
- Add command notification for folder and drive selection changes
- Add conflict handling settings and drive content checks for transfers
- Implement transfer history feature with view model, service, and UI components

### Fixed

- Update CHANGELOG.md to reflect recent enhancements and fixes in retention cleanup functionality and UI improvements
- Fix HelpWindow icon path to ensure correct application icon display
- Fix file viewability check in FileService
- Change log level from Info to Debug for drive detection
- Fix project file formatting
- Fix validation message for folder name pattern and update binding for compressed audit status icon in MainWindow
- Fix HTML entity in Compliance Records group header in SettingsWindow.xaml

### Changed

- Refine warning and error detection in code quality check script for improved accuracy; enhance test existence check condition
- Refactor string handling to use Invariant culture for consistency across converters and services; update List to IList for better abstraction in models and services
- Refactor file handling logic in MainViewModel for improved consistency; update resource dictionary comments and implement IDisposable pattern in services
- Update CHANGELOG.md with correct version dates for releases 1.3.2, 1.3.0, 1.2.0, 1.1.0, and 1.0.0
- Implement InverseBooleanToVisibilityConverter and integrate retention cleanup functionality in MainWindow
- Enhance MarkdownViewer styling in ChangesWindow: adjust margins for paragraphs, sections, and lists to improve spacing and layout
- Adjust footer margin in ChangesWindow for improved layout
- Update CHANGELOG.md to version 1.3.3, reorganize resources, and enhance ChangesViewModel for embedded resource loading
- Bump version to 1.3.2 and update release notes with recent changes and improvements
- Refactor ViewModels to inherit from ViewModelBase and update UI elements for consistency
- Refactor icon usage in FileViewer and Settings windows: replace Path elements with FontAwesome icons for improved UI consistency
- Change compliance file naming format
- Change DataGridTextColumn width for File Name
- Change drive detection timer interval and make method async
- Complete implementation of all services, view models, and views; update package versions and project status
- Update version to 1.3.0 and enhance version management with VersionHelper
- Enhance text file detection to always consider .md files as text and improve UTF-8 validation logic
- Enhance file viewability check to include content analysis for unknown extensions
- Refactor code structure for improved readability and maintainability
- Update compliance record settings and enhance logging in TransferService
- Update simple.txt: expand Standard section and enhance Transfer Compliance Record details
- Refactor Excluded Folders section in SettingsWindow: simplify UI and remove button functionalities
- Change ResetButton background to WarningBrush
- Change TextBlock styles to FootnoteStyle
- Refactor folder exclusion logic in FileService
- Refactor excluded folder handling in TransferService
- Refactor drive scanning and deletion logic
- Upgrade target framework and package versions
- Update EPPlus license context setting
- Expand ExcludeDrives to include C: and D:
- Update whitelist datasets in AppSettings
- Clean up blank lines in TransferService.cs
- Adjust ExitButton dimensions and icon size for improved UI consistency
- Update ArchiveViewerWindow layout and adjust DataGrid column widths for better visibility
- Reorganize App.xaml by moving ScrollBar size definitions and removing commented-out geometry
- Refactor validation messages for consistency and update MainWindow UI to display folder naming audit status
- Enhance logging for transfer database operations; improve error handling and success messages in TransferDatabaseService and TransferService
- Refactor compliance record handling and database interactions; remove unused path settings and streamline TransferHistoryService
- Remove transfer logging settings from SettingsWindow.xaml
- Refactor transfer logging settings and enhance compliance record generation; update UI for database storage options and compliance record settings
- Update EPPlus package to version 7.7.0 and improve error handling in TransferHistoryService for transfer retrieval
- Merge branch 'main' of <https://github.com/Zephynyah/DataTransferApp.Net>
- Refactor transfer logging to transfer records; update related directory paths and settings in the application
- Remove default value for Trimmed parameter
- Refactor Build-Executable.ps1 parameters
- Rename build-exe.ps1 to Build-Executable.ps1
- Change 'published' to 'publish' in .gitignore
- Refactor geometry definitions in App.xaml; update color mappings in converters; modify application settings paths; enhance logging messages; adjust UI element properties in MainWindow and AboutViewWindow
- Update .gitignore to include 'docs/' and 'published/' directories
- Set RowHeight for DataGrids in ArchiveViewer and MainWindow to improve readability
- Enhance archive handling to support compound archives using Reader approach; improve extraction logic for various archive types
- Refactor file and folder models to use observable properties; implement audit status converters and enhance UI with new data bindings for audit results
- Update Dropbox icon geometry and adjust button icon sizes for improved UI consistency
- Refactor MainWindow layout and update button styles for improved UI consistency
- Update UI elements in MainWindow and SettingsWindow for improved layout and styling
- Refactor UI styles and enhance transfer logging settings
- Enhance drive clearing process with detailed progress reporting
- Update styles and resources in XAML files; remove solution file
- Initilal commit
