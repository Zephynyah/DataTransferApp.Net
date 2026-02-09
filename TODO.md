# TODO

## RoboSharp Integration Improvements

### Progress Tracking

- [x] **Progress is being tracked by RoboSharp** via events:
  - `OnProgressEstimatorCreated` - Provides total files/bytes estimates
  - `OnFileProcessed` - Reports each completed file with size
  - `OnCopyProgressChanged` - Reports individual file copy progress
  - `RoboSharpProgressAdapter` translates events to `TransferProgress` model
- [x] Verify real-time progress updates in UI (test with large transfers)
- [x] Add MB/s speed calculation validation
- [x] Test ETA accuracy with various file sizes
- [x] Implement cancel functionality for active transfers
- [ ] Implement pause/resume functionality using RoboSharp job files (in `feature-pause-resume` branch)

### Settings & Configuration

- [x] Add RoboSharp settings UI in SettingsWindow.xaml
  - Thread count slider (1-128, default 8)
  - Retry count/wait time  
  - Enable/disable verbose logging
  - Enable/disable transfer logging
  - Mirror mode toggle (future)
  - Purge destination option (future)
- [x] Add "Use RoboSharp" toggle in Settings (applies to all transfers)
- [x] Create preset configurations (Fast, Safe, Network, Archive)

### Error Handling & Logging

- [x] **Enhanced TransferSummary model** with RoboSharp statistics:
  - TransferMethod (Legacy/RoboSharp)
  - RobocopyExitCode, FilesCopied, FilesSkipped, FilesFailed
  - DirectoriesCopied, BytesCopied, AverageSpeedBytesPerSecond
  - Errors list, FormattedSpeed, Duration
- [x] **Enhanced TransferLog model** with computed properties:
  - IsRoboSharpTransfer, HasErrors, FirstError
- [x] **Updated TransferService** to populate RoboSharp statistics in transfer logs
- [x] **Enhanced TransferHistoryWindow UI** to display:
  - RoboSharp transfer statistics (speed, counts, duration)
  - Exit codes and error messages
  - Visual distinction for RoboSharp vs Legacy transfers
- [x] Add retry logic with exponential backoff
- [x] Add detailed error tooltips with file paths in FileData grid
- [ ] Create error summary report for bulk transfers

### Testing & Validation

- [x] Create unit tests for RoboSharpTransferEngine (✅ 24 integration tests)
- [x] Create unit tests for RetryHelper (✅ 16 unit tests)
- [ ] Test network transfer scenarios (slow/unreliable connections)
- [ ] Test with very large files (>10GB)
- [ ] Test with many small files (>10,000)
- [ ] Benchmark RoboSharp vs legacy transfer method
- [x] Test cancel functionality during active transfers (✅ Tested and working)

### Performance Optimization

- [ ] Profile memory usage during large transfers
- [ ] Optimize progress update frequency (currently 500ms)
- [ ] Consider batching small file operations
- [ ] Test multi-threaded copy performance (1, 8, 16, 32 threads)

### Documentation

- [ ] Update user documentation with RoboSharp features
- [ ] Document when to use RoboSharp vs legacy method
- [ ] Add troubleshooting guide for common Robocopy errors
- [ ] Create migration guide from legacy to RoboSharp

## General App Improvements

- [ ] Add dark mode theme
- [ ] Implement drag-and-drop for file selection
- [ ] Add transfer queue management
- [ ] Create scheduled transfer capability
- [ ] Add network drive mapping assistant
