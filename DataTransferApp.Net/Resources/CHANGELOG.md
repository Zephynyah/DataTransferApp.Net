# Changelog

All notable changes to Data Transfer Application (.NET) will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.2] - 2026-01-30

### Fixed
- Resolved issues with file hash calculation for large files
- Fixed memory leaks during bulk file operations

### Added
- PDF export option for compliance reports
- Enhanced progress bar animations and status indicators

### Security
- Improved path validation to prevent directory traversal attacks

### Performance
- Optimized memory usage during bulk file operations
- Improved file detection algorithms

## [1.3.0] - 2026-01-28

### Added
- Enhanced file detection with special handling for .md files
- Better detection of viewable files using content analysis
- Archive files are now properly marked as viewable for content browsing

### Performance
- Sample-based file reading for faster detection
- Improved loading times for large directories

## [1.2.0] - 2026-01-21

### Added
- Transfer History Database with 50-200x performance improvement
- Automated compliance records generation (CSV/Excel)
- Multi-user support with shared database
- Migration tools for importing existing transfer logs

### Changed
- Database backend for transfer history storage
- Improved concurrent user handling

## [1.1.0] - 2026-01-17

### Added
- Archive file support with preview and validation
- Real-time transfer progress statistics
- Automatic settings persistence

### Enhanced
- Folder naming and file type validation
- Progress reporting system

## [1.0.0] - 2026-01-10

### Added
- Initial release of Data Transfer Application (.NET)
- Air-gapped data transfer functionality
- Folder auditing and validation
- Compliance record generation
- Transfer history tracking
- Configurable settings and validation rules
- WPF user interface with MVVM architecture

### Security
- Path validation to prevent unauthorized access
- File type restrictions and dataset authorization
- Audit trail generation for all transfers</content>
<parameter name="filePath">d:\Powershell\GUI\DTA-WPF\DataTransferApp.Net\CHANGES.md