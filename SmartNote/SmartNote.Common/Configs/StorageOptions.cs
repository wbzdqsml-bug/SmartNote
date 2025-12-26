namespace SmartNote.Common.Configs
{
    public class StorageOptions
    {
        public long MaxAvatarSizeBytes { get; set; } = 2 * 1024 * 1024;
        public long MaxAttachmentSizeBytes { get; set; } = 20 * 1024 * 1024;
    }
}
