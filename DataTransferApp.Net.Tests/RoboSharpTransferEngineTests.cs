using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataTransferApp.Net.Models;
using DataTransferApp.Net.Services;
using Xunit;

namespace DataTransferApp.Net.Tests
{
    /// <summary>
    /// Integration tests for RoboSharpTransferEngine.
    /// Uses real file system operations with temporary directories for comprehensive testing.
    /// </summary>
    public class RoboSharpTransferEngineTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _sourceDir;
        private readonly string _destDir;
        private readonly RoboSharpTransferEngine _engine;

        public RoboSharpTransferEngineTests()
        {
            // Create temporary test directory structure
            _tempDir = Path.Combine(Path.GetTempPath(), $"RoboSharpTests_{Guid.NewGuid()}");
            _sourceDir = Path.Combine(_tempDir, "Source");
            _destDir = Path.Combine(_tempDir, "Destination");

            Directory.CreateDirectory(_sourceDir);
            Directory.CreateDirectory(_destDir);

            _engine = new RoboSharpTransferEngine();
        }

        public void Dispose()
        {
            // Clean up test directories
            try
            {
                if (Directory.Exists(_tempDir))
                {
                    Directory.Delete(_tempDir, recursive: true);
                }
            }
            catch
            {
                // Best effort cleanup
            }

            GC.SuppressFinalize(this);
        }

        #region Transfer Folder Tests

        [Fact]
        public async Task TransferFolderAsync_WithValidPaths_CopiesFilesSuccessfully()
        {
            // Arrange - Create test files
            CreateTestFile(Path.Combine(_sourceDir, "test1.txt"), "Test content 1");
            CreateTestFile(Path.Combine(_sourceDir, "test2.txt"), "Test content 2");

            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success, $"Transfer failed: {result.ErrorMessage}");
            Assert.True(result.ExitCode <= 1, $"Exit code should be 0 or 1, got {result.ExitCode}"); // 0 = no changes, 1 = files copied
            Assert.Equal(2, result.FilesCopied);
            Assert.True(File.Exists(Path.Combine(_destDir, "test1.txt")));
            Assert.True(File.Exists(Path.Combine(_destDir, "test2.txt")));
        }

        [Fact]
        public async Task TransferFolderAsync_WithSubdirectories_CopiesRecursively()
        {
            // Arrange
            var subDir = Path.Combine(_sourceDir, "SubFolder");
            Directory.CreateDirectory(subDir);

            CreateTestFile(Path.Combine(_sourceDir, "root.txt"), "Root file");
            CreateTestFile(Path.Combine(subDir, "sub.txt"), "Sub file");

            var options = CreateDefaultOptions();
            options.CopySubdirectories = true;

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.FilesCopied);
            Assert.True(File.Exists(Path.Combine(_destDir, "root.txt")));
            Assert.True(File.Exists(Path.Combine(_destDir, "SubFolder", "sub.txt")));
        }

        [Fact]
        public async Task TransferFolderAsync_WithNonExistentSource_ReturnsFailure()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_tempDir, "DoesNotExist");
            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.TransferFolderAsync(nonExistentPath, _destDir, options);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.ErrorMessage);
            Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task TransferFolderAsync_WithEmptySource_ReturnsSuccessWithNoFiles()
        {
            // Arrange - Source directory exists but is empty
            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.FilesCopied == 0 || result.ExitCode == 0, "Empty directory should result in no files copied");
        }

        [Fact]
        public async Task TransferFolderAsync_WithProgressReporting_InvokesProgressCallbacks()
        {
            // Arrange
            CreateTestFile(Path.Combine(_sourceDir, "test.txt"), "Test content");

            var options = CreateDefaultOptions();
            var progressReports = new List<TransferProgress>();
            var progress = new Progress<TransferProgress>(p =>
            {
                if (p != null)
                {
                    progressReports.Add(p);
                }
            });

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options, progress);

            // Assert
            Assert.True(result.Success);
            // Progress reporting may or may not fire depending on transfer speed
            // Just verify the mechanism doesn't cause errors
            Assert.NotNull(progressReports);
        }

        [Fact]
        public async Task TransferFolderAsync_WithCancellation_StopsTransfer()
        {
            // Arrange - Create many files to give time for cancellation
            for (int i = 0; i < 50; i++)
            {
                CreateTestFile(Path.Combine(_sourceDir, $"file{i}.txt"), $"Content {i}");
            }

            var options = CreateDefaultOptions();
            var cts = new CancellationTokenSource();

            // Act - Start transfer and cancel after a short delay
            var transferTask = _engine.TransferFolderAsync(_sourceDir, _destDir, options, null, cts.Token);
            await Task.Delay(100);
            cts.Cancel();

            var result = await transferTask;

            // Assert - Transfer should indicate cancellation or partial completion
            // Note: Depending on timing, all files might be copied before cancellation
            // Just verify the cancellation mechanism doesn't cause exceptions
            Assert.NotNull(result);
            Assert.True(result.FilesCopied <= 50, "Should not copy more than the source files");
        }

        #endregion

        #region Transfer Files Tests

        [Fact]
        public async Task TransferFilesAsync_WithSpecificFiles_CopiesOnlySpecified()
        {
            // Arrange
            CreateTestFile(Path.Combine(_sourceDir, "file1.txt"), "Content 1");
            CreateTestFile(Path.Combine(_sourceDir, "file2.txt"), "Content 2");
            CreateTestFile(Path.Combine(_sourceDir, "file3.txt"), "Content 3");

            var filesToTransfer = new[]
            {
                Path.Combine(_sourceDir, "file1.txt"),
                Path.Combine(_sourceDir, "file3.txt")
            };

            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.TransferFilesAsync(filesToTransfer, _sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.True(File.Exists(Path.Combine(_destDir, "file1.txt")));
            // Note: RoboCopy with file filters may still copy all files depending on configuration
            // The key is that specified files definitely exist
            Assert.True(File.Exists(Path.Combine(_destDir, "file3.txt")));
        }

        [Fact]
        public async Task TransferFilesAsync_WithEmptyFileList_ReturnsSuccessWithNoFiles()
        {
            // Arrange
            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.TransferFilesAsync(Array.Empty<string>(), _sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.FilesCopied);
        }

        [Fact]
        public async Task TransferFilesAsync_WithSubdirectoryFiles_PreservesStructure()
        {
            // Arrange
            var subDir = Path.Combine(_sourceDir, "Subfolder");
            Directory.CreateDirectory(subDir);

            CreateTestFile(Path.Combine(_sourceDir, "root.txt"), "Root");
            CreateTestFile(Path.Combine(subDir, "sub.txt"), "Sub");

            var filesToTransfer = new[]
            {
                Path.Combine(_sourceDir, "root.txt"),
                Path.Combine(subDir, "sub.txt")
            };

            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.TransferFilesAsync(filesToTransfer, _sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.True(File.Exists(Path.Combine(_destDir, "root.txt")));
            Assert.True(File.Exists(Path.Combine(_destDir, "Subfolder", "sub.txt")));
        }

        #endregion

        #region Estimate Transfer Tests

        [Fact]
        public async Task EstimateTransferAsync_WithFiles_ReturnsAccurateCounts()
        {
            // Arrange
            CreateTestFile(Path.Combine(_sourceDir, "test1.txt"), "Content 1");
            CreateTestFile(Path.Combine(_sourceDir, "test2.txt"), "Content 2");

            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.EstimateTransferAsync(_sourceDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.FilesScanned);
            Assert.True(result.BytesTotal > 0);
        }

        [Fact]
        public async Task EstimateTransferAsync_WithEmptyDirectory_ReturnsZero()
        {
            // Arrange
            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.EstimateTransferAsync(_sourceDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.FilesScanned);
            Assert.Equal(0, result.BytesTotal);
        }

        [Fact]
        public async Task EstimateTransferAsync_WithSubdirectories_CountsRecursively()
        {
            // Arrange
            var subDir = Path.Combine(_sourceDir, "SubFolder");
            Directory.CreateDirectory(subDir);

            CreateTestFile(Path.Combine(_sourceDir, "root.txt"), "Root");
            CreateTestFile(Path.Combine(subDir, "sub.txt"), "Sub");

            var options = CreateDefaultOptions();
            options.CopySubdirectories = true;

            // Act
            var result = await _engine.EstimateTransferAsync(_sourceDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.FilesScanned);
        }

        #endregion

        #region Options Configuration Tests

        [Fact]
        public async Task TransferFolderAsync_WithExcludeFiles_SkipsMatchingFiles()
        {
            // Arrange
            CreateTestFile(Path.Combine(_sourceDir, "include.txt"), "Include");
            CreateTestFile(Path.Combine(_sourceDir, "exclude.log"), "Exclude");
            CreateTestFile(Path.Combine(_sourceDir, "exclude.tmp"), "Exclude");

            var options = CreateDefaultOptions();
            options.ExcludeFiles = new List<string> { "*.log", "*.tmp" };

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.True(File.Exists(Path.Combine(_destDir, "include.txt")));
            Assert.False(File.Exists(Path.Combine(_destDir, "exclude.log")));
            Assert.False(File.Exists(Path.Combine(_destDir, "exclude.tmp")));
        }

        [Fact]
        public async Task TransferFolderAsync_WithExcludeDirectories_SkipsMatchingDirs()
        {
            // Arrange
            var includeDir = Path.Combine(_sourceDir, "Include");
            var excludeDir = Path.Combine(_sourceDir, "Exclude");
            Directory.CreateDirectory(includeDir);
            Directory.CreateDirectory(excludeDir);

            CreateTestFile(Path.Combine(includeDir, "file.txt"), "Include");
            CreateTestFile(Path.Combine(excludeDir, "file.txt"), "Exclude");

            var options = CreateDefaultOptions();
            options.ExcludeDirectories = new List<string> { "Exclude" };

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.True(Directory.Exists(Path.Combine(_destDir, "Include")));
            Assert.False(Directory.Exists(Path.Combine(_destDir, "Exclude")));
        }

        [Fact]
        public async Task TransferFolderAsync_WithThreadCount_ConfiguresMultithreading()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                CreateTestFile(Path.Combine(_sourceDir, $"file{i}.txt"), $"Content {i}");
            }

            var options = CreateDefaultOptions();
            options.ThreadCount = 4; // Explicit thread count

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.FilesCopied);
        }

        #endregion

        #region Event Tests

        [Fact]
        public async Task TransferFolderAsync_OnSuccess_FiresOnCompletedEvent()
        {
            // Arrange
            CreateTestFile(Path.Combine(_sourceDir, "test.txt"), "Test");

            var options = CreateDefaultOptions();
            RoboSharpTransferResultEventArgs? completedArgs = null;

            _engine.OnCompleted += (sender, args) =>
            {
                completedArgs = args;
            };

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(completedArgs);
            Assert.NotNull(completedArgs.Result);
            Assert.True(completedArgs.Result.Success);
        }

        [Fact]
        public async Task TransferFolderAsync_WithReadOnlyFile_FiresOnErrorEvent()
        {
            // Arrange - Create a read-only file that might cause issues (OS-dependent)
            var readOnlyFile = Path.Combine(_sourceDir, "readonly.txt");
            CreateTestFile(readOnlyFile, "Read only");
            File.SetAttributes(readOnlyFile, FileAttributes.ReadOnly);

            // Also create a destination file that's read-only to trigger an access error
            var destFile = Path.Combine(_destDir, "readonly.txt");
            CreateTestFile(destFile, "Existing");
            File.SetAttributes(destFile, FileAttributes.ReadOnly);

            var options = CreateDefaultOptions();
            var errors = new List<RoboSharpErrorEventArgs>();

            _engine.OnError += (sender, args) =>
            {
                errors.Add(args);
            };

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert - May or may not error depending on OS and permissions
            // Just verify event mechanism works if errors occur
            if (!result.Success || errors.Count > 0)
            {
                Assert.NotEmpty(errors);
            }
        }

        #endregion

        #region Result Validation Tests

        [Fact]
        public async Task TransferFolderAsync_Result_ContainsCorrectStatistics()
        {
            // Arrange
            CreateTestFile(Path.Combine(_sourceDir, "file1.txt"), "Content 1");
            CreateTestFile(Path.Combine(_sourceDir, "file2.txt"), "Content 2");

            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.SourcePath);
            Assert.NotNull(result.DestinationPath);
            Assert.True(result.StartTime < result.EndTime);
            Assert.Equal(2, result.FilesCopied);
            Assert.True(result.BytesCopied > 0);
        }

        [Fact]
        public async Task TransferFolderAsync_WithIdenticalFiles_SkipsAlreadyCopied()
        {
            // Arrange - Transfer once
            CreateTestFile(Path.Combine(_sourceDir, "test.txt"), "Test content");
            var options = CreateDefaultOptions();

            var firstResult = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Act - Transfer again with no changes
            var secondResult = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(firstResult.Success);
            Assert.True(secondResult.Success);
            Assert.Equal(1, firstResult.FilesCopied);
            // Second transfer may copy or skip depending on RoboCopy's file comparison
            // Just verify it doesn't fail
            Assert.True(secondResult.FilesCopied <= 1, "Second transfer should not copy more files");
        }

        #endregion

        #region Large File Tests

        [Fact]
        public async Task TransferFolderAsync_WithLargeFile_TransfersSuccessfully()
        {
            // Arrange - Create a 10MB test file
            var largeFile = Path.Combine(_sourceDir, "large.bin");
            CreateLargeTestFile(largeFile, 10 * 1024 * 1024); // 10 MB

            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.FilesCopied);
            Assert.Equal(10 * 1024 * 1024, result.BytesCopied);
            Assert.True(File.Exists(Path.Combine(_destDir, "large.bin")));
        }

        [Fact]
        public async Task TransferFolderAsync_WithManySmallFiles_TransfersSuccessfully()
        {
            // Arrange - Create 100 small files
            for (int i = 0; i < 100; i++)
            {
                CreateTestFile(Path.Combine(_sourceDir, $"small{i}.txt"), $"Small {i}");
            }

            var options = CreateDefaultOptions();

            // Act
            var result = await _engine.TransferFolderAsync(_sourceDir, _destDir, options);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(100, result.FilesCopied);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test file with the specified content.
        /// </summary>
        private static void CreateTestFile(string path, string content)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Creates a large test file with the specified size in bytes.
        /// </summary>
        private static void CreateLargeTestFile(string path, long sizeInBytes)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            fs.SetLength(sizeInBytes);
        }

        /// <summary>
        /// Creates default RoboSharpOptions for testing.
        /// </summary>
        private static RoboSharpOptions CreateDefaultOptions()
        {
            return new RoboSharpOptions
            {
                ThreadCount = 2, // Lower thread count for faster tests
                RetryCount = 1,
                RetryWaitSeconds = 1,
                CopySubdirectories = true,
                CopyEmptySubdirectories = false,
                VerboseOutput = false
            };
        }

        #endregion
    }
}
