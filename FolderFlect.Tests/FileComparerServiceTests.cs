using NUnit.Framework;
using FolderFlect.Models;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NLog;

[TestFixture]
public class FileComparerServiceTests
{
    private IFileComparerService _service;
    private Mock<ILogger> _mockLogger;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger>();
        _service = new FileComparerService(_mockLogger.Object);
    }

    [Test]
    // Test to ensure directories that exist in the destination but not in the source are marked for deletion.
    public void DetermineSyncActionsByMD5_ShouldMarkMissingSourceDirectoriesForDeletion()
    {
        // Arrange
        var fileSet = new MD5FileSet(
            Enumerable.Empty<FileModel>().ToLookup(f => f.MD5Hash),  // Source Files
            Enumerable.Empty<FileModel>().ToLookup(f => f.MD5Hash),  // Destination Files
            new Dictionary<string, string> { { "path1", "path1/path1" } }, // Source Directories
            new Dictionary<string, string> { { "path1", "path1/path1" }, { "path2", "path2/path2" } } // Destination Directories
        );

        // Act
        var result = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        Assert.Contains("path2", result.Value.DirectoriesToDelete);
    }

    [Test]
    // Test to ensure directories that exist in the destination but not in the source are marked for deletion.
    public void DetermineDirectoriesToCreateMD5_ShouldMarkMissingDestDirectoriesForCreation()
    {
        // Arrange
        var fileSet = new MD5FileSet(
            Enumerable.Empty<FileModel>().ToLookup(f => f.MD5Hash),  // Source Files
            Enumerable.Empty<FileModel>().ToLookup(f => f.MD5Hash),  // Destination Files
            new Dictionary<string, string> { { "path1", "path1/path1" }, { "path2", "path2/path2" } }, // Source Directories
            new Dictionary<string, string> { { "path1", "path1/path1" } } // Destination Directories
        );

        // Act
        var result = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        Assert.Contains("path2", result.Value.DirectoriesToCreate);
    }

    [Test]
    public void DetermineFilesToDeleteMD5_WhenHashExistsInDestinationButNotInSource_ShouldReturnFilesForDeletion()
    {
        // Arrange

        var destinationFile = new FileModel
        {
            MD5Hash = "sampleMD5Hash",
            FileReletivePath = "sample/path/to/destination/file.ext"
        };

        var destinationFile1 = new FileModel
        {
            MD5Hash = "sampleMD5Hash1",
            FileReletivePath = "sample/path/to/destination/file1.ext"
        };

        var fileSet = new MD5FileSet(
            new[] { destinationFile1, }.ToLookup(f => f.MD5Hash),  // Source Files
            new[] { destinationFile, destinationFile1 }.ToLookup(f => f.MD5Hash),      // Destination Files
            new Dictionary<string, string>(), // Source Directories
            new Dictionary<string, string>()  // Destination Directories
        );

        // Act
        var filesToDelete = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        Assert.IsTrue(filesToDelete.Value.FilesToDelete.Contains(destinationFile.FileReletivePath));

    }

    [Test]
    public void DetermineFilesToCopyMD5_ShouldMarkMissingDestFilesForCopying()
    {
        // Arrange
        var sourceFile = new FileModel
        {
            MD5Hash = "copyMD5Hash",
            FileReletivePath = "sample/path/to/source/fileToCopy.ext"
        };

        var sourceFile1 = new FileModel
        {
            MD5Hash = "copyMD5Hash1",
            FileReletivePath = "sample/path/to/source/fileToCopy1.ext"
        };

        var fileSet = new MD5FileSet(
            new[] { sourceFile, sourceFile1 }.ToLookup(f => f.MD5Hash),  // Source Files
            new[] { sourceFile1 }.ToLookup(f => f.MD5Hash),             // Destination Files
            new Dictionary<string, string>(), // Source Directories
            new Dictionary<string, string>()  // Destination Directories
        );

        // Act
        var filesToSync = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        Assert.IsTrue(filesToSync.Value.FilesToCopy.Contains(sourceFile.FileReletivePath));
    }

    [Test]
    public void DetermineFilesToMoveMD5_ShouldMarkFilesForMovementBasedOnPath()
    {
        // Arrange
        var sourceFile = new FileModel
        {
            MD5Hash = "moveMD5Hash",
            FileReletivePath = "sample/path/to/source/fileToMove.ext"
        };

        var destFile = new FileModel
        {
            MD5Hash = "moveMD5Hash",
            FileReletivePath = "sample/oldpath/to/destination/fileToMove.ext"
        };

        var fileSet = new MD5FileSet(
            new[] { sourceFile }.ToLookup(f => f.MD5Hash),
            new[] { destFile }.ToLookup(f => f.MD5Hash),
            new Dictionary<string, string>(),
            new Dictionary<string, string>()
        );

        // Act
        var filesToSync = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        var moveTuple = (destFile.FileReletivePath, sourceFile.FileReletivePath);
        Assert.IsTrue(filesToSync.Value.FilesToMove.Contains(moveTuple));
    }

    [Test]
    public void DetermineFilesToCopyMD5_WhenMoreDuplicateSourceFilesThanDest_ShouldMarkExtraSourceFilesForCopying()
    {
        // Arrange
        var sourceFile = new FileModel
        {
            MD5Hash = "duplicateMD5Hash",
            FileReletivePath = "sample/path/to/source/file1.ext"
        };

        var sourceFileDuplicate = new FileModel
        {
            MD5Hash = "duplicateMD5Hash",
            FileReletivePath = "sample/path/to/source/file2.ext"
        };

        var destFile = new FileModel
        {
            MD5Hash = "duplicateMD5Hash",
            FileReletivePath = "sample/path/to/destination/file.ext"
        };

        var fileSet = new MD5FileSet(
            new[] { sourceFile, sourceFileDuplicate }.ToLookup(f => f.MD5Hash),   // Source Files
            new[] { destFile }.ToLookup(f => f.MD5Hash),                          // Destination Files
            new Dictionary<string, string>(),                                    // Source Directories
            new Dictionary<string, string>()                                      // Destination Directories
        );

        // Act
        var filesToSync = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        Assert.IsTrue(filesToSync.Value.FilesToCopy.Contains(sourceFileDuplicate.FileReletivePath));
    }

    [Test]
    public void DetermineFilesToDeleteMD5_WhenMoreDuplicateDestFilesThanSource_ShouldMarkExtraDestFilesForDeletion()
    {
        // Arrange
        var sourceFile = new FileModel
        {
            MD5Hash = "duplicateMD5Hash",
            FileReletivePath = "sample/path/to/source/file.ext"
        };

        var destFile = new FileModel
        {
            MD5Hash = "duplicateMD5Hash",
            FileReletivePath = "sample/path/to/destination/file1.ext"
        };

        var destFileDuplicate = new FileModel
        {
            MD5Hash = "duplicateMD5Hash",
            FileReletivePath = "sample/path/to/destination/file2.ext"
        };

        var fileSet = new MD5FileSet(
            new[] { sourceFile }.ToLookup(f => f.MD5Hash),                         // Source Files
            new[] { destFile, destFileDuplicate }.ToLookup(f => f.MD5Hash),       // Destination Files
            new Dictionary<string, string>(),                                     // Source Directories
            new Dictionary<string, string>()                                       // Destination Directories
        );

        // Act
        var filesToSync = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        Assert.IsTrue(filesToSync.Value.FilesToDelete.Contains(destFileDuplicate.FileReletivePath));
    }

    [Test]
    public void DetermineSyncActionsByMD5_WhenSourceAndDestinationAreIdentical_ShouldReturnNoActions()
    {
        // Arrange
        var file = new FileModel
        {
            MD5Hash = "sampleMD5Hash",
            FileReletivePath = "sample/path/to/file.ext"
        };

        var fileSet = new MD5FileSet(
            new[] { file }.ToLookup(f => f.MD5Hash),
            new[] { file }.ToLookup(f => f.MD5Hash),
            new Dictionary<string, string> { { "path", "path/path" } },
            new Dictionary<string, string> { { "path", "path/path" } }
        );

        // Act
        var filesToSync = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        Assert.IsEmpty(filesToSync.Value.FilesToMove);
        Assert.IsEmpty(filesToSync.Value.FilesToDelete);
        Assert.IsEmpty(filesToSync.Value.FilesToCopy);
        Assert.IsEmpty(filesToSync.Value.DirectoriesToCreate);
        Assert.IsEmpty(filesToSync.Value.DirectoriesToDelete);
    }

    [Test]
    public void DetermineSyncActionsByMD5_WhenPathsAreSameButHashesDiffer_ShouldMarkFilesForCopyAndDeletion()
    {
        // Arrange
        var sourceFile = new FileModel
        {
            MD5Hash = "sourceMD5Hash",
            FileReletivePath = "sample/path/to/file.ext"
        };

        var destFile = new FileModel
        {
            MD5Hash = "destMD5Hash",
            FileReletivePath = "sample/path/to/file.ext"
        };

        var fileSet = new MD5FileSet(
            new[] { sourceFile }.ToLookup(f => f.MD5Hash),
            new[] { destFile }.ToLookup(f => f.MD5Hash),
            new Dictionary<string, string>(),
            new Dictionary<string, string>()
        );

        // Act
        var filesToSync = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        Assert.IsTrue(filesToSync.Value.FilesToDelete.Contains(destFile.FileReletivePath));
        Assert.IsTrue(filesToSync.Value.FilesToCopy.Contains(sourceFile.FileReletivePath));
    }

    [Test]
    public void DetermineSyncActionsByMD5_WhenSourceAndDestinationAreEmpty_ShouldReturnNoActions()
    {
        // Arrange
        var fileSet = new MD5FileSet(
            Enumerable.Empty<FileModel>().ToLookup(f => f.MD5Hash),
            Enumerable.Empty<FileModel>().ToLookup(f => f.MD5Hash),
            new Dictionary<string, string>(),
            new Dictionary<string, string>()
        );

        // Act
        var filesToSync = _service.GetFilesToSyncGroupedByMD5AndDirectoryPaths(fileSet);

        // Assert
        Assert.IsEmpty(filesToSync.Value.FilesToMove);
        Assert.IsEmpty(filesToSync.Value.FilesToDelete);
        Assert.IsEmpty(filesToSync.Value.FilesToCopy);
        Assert.IsEmpty(filesToSync.Value.DirectoriesToCreate);
        Assert.IsEmpty(filesToSync.Value.DirectoriesToDelete);
    }

}

