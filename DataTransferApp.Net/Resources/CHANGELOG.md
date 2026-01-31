# Changelog

All notable changes to Data Transfer Application (.NET) will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


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