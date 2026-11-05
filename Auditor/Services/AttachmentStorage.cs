namespace Auditor.Services
{
    /// <summary>
    /// Stores audit evidence (photos, documents) outside wwwroot so files are only served
    /// through an authorised controller action. In Docker the folder is a volume.
    /// </summary>
    public class AttachmentStorage
    {
        public const long MaxFileBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".mp4", ".mov",
        };

        private readonly string _root;

        public AttachmentStorage(IConfiguration config, IWebHostEnvironment env)
        {
            _root = config["Storage:UploadsPath"] is { Length: > 0 } configured
                ? configured
                : Path.Combine(env.ContentRootPath, "App_Data", "uploads");
            Directory.CreateDirectory(_root);
        }

        /// <summary>Returns an error message, or null when the file can be stored.</summary>
        public static string? Validate(IFormFile file)
        {
            if (file.Length == 0)
                return $"\"{file.FileName}\" is empty.";
            if (file.Length > MaxFileBytes)
                return $"\"{file.FileName}\" is larger than {MaxFileBytes / 1024 / 1024} MB.";
            if (!AllowedExtensions.Contains(Path.GetExtension(file.FileName)))
                return $"\"{file.FileName}\" isn't an allowed file type (photos, PDF, Office documents or video).";
            return null;
        }

        /// <summary>Saves the file and returns its storage key (relative path).</summary>
        public async Task<string> SaveAsync(long executionId, long responseId, IFormFile file)
        {
            var folder = Path.Combine(_root, executionId.ToString());
            Directory.CreateDirectory(folder);

            var key = $"{executionId}/{responseId}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
            await using var stream = new FileStream(Path.Combine(_root, key), FileMode.CreateNew);
            await file.CopyToAsync(stream);
            return key;
        }

        /// <summary>Resolves a storage key to a full path, refusing anything outside the uploads folder.</summary>
        public string? Resolve(string key)
        {
            // Older records stored a public URL (/uploads/...); map it onto the same folder layout.
            key = key.Replace("/uploads/", "", StringComparison.OrdinalIgnoreCase).TrimStart('/');
            var full = Path.GetFullPath(Path.Combine(_root, key));
            return full.StartsWith(Path.GetFullPath(_root), StringComparison.Ordinal) && File.Exists(full) ? full : null;
        }
    }
}
