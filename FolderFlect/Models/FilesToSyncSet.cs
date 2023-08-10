namespace FolderFlect.Models
{
    public record FilesToSyncSet
    {
        public FilesToSyncSet(
            List<string> directoriesToDelete,
            List<string> filesToDelete,
            List<string> filesToCopy,
            List<string> directoriesToCreate,
            List<string> filesForUpdate,
            List<string> filesForAttributesUpdate)
        {
            DirectoriesToDelete = directoriesToDelete;
            FilesToDelete = filesToDelete;
            FilesToCopy = filesToCopy;
            DirectoriesToCreate = directoriesToCreate;
            FilesForUpdate = filesForUpdate;
            FilesForAttributesUpdate = filesForAttributesUpdate;
        }

        public List<string> DirectoriesToDelete { get; init; }
        public List<string> FilesToDelete { get; init; }
        public List<string> FilesToCopy { get; init; }
        public List<string> DirectoriesToCreate { get; init; }
        public List<string> FilesForUpdate { get; init; }
        public List<string> FilesForAttributesUpdate { get; init; }
    }
}
