using DataTransferApp.Net.Services;

namespace DataTransferApp.Net.Tests;

public class FileServiceTests : IDisposable
{
    private readonly string _tempDir;

    public FileServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public void IsFileViewable_WithViewableExtensionAndTextFile_ReturnsTrue()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "This is a text file");
        string extension = ".txt";

        // Act
        bool result = FileService.IsFileViewable(filePath, extension);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsFileViewable_WithNonViewableExtension_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "test.exe");
        // Write binary data that won't be detected as text
        byte[] binaryData = new byte[1024];
        for (int i = 0; i < binaryData.Length; i++)
        {
            binaryData[i] = (byte)(i % 256);
        }
        File.WriteAllBytes(filePath, binaryData);
        string extension = ".exe";

        // Act
        bool result = FileService.IsFileViewable(filePath, extension);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsFileViewable_WithEmptyExtension_ReturnsFalse()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "test");
        // Write binary data that won't be detected as text
        byte[] binaryData = new byte[1024];
        for (int i = 0; i < binaryData.Length; i++)
        {
            binaryData[i] = (byte)(i % 256);
        }
        File.WriteAllBytes(filePath, binaryData);
        string extension = "";

        // Act
        bool result = FileService.IsFileViewable(filePath, extension);

        // Assert
        Assert.False(result);
    }
}