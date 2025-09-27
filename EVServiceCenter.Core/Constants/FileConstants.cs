namespace EVServiceCenter.Core.Constants
{
    public static class FileConstants
    {
        public const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
        public const string UPLOAD_PATH = "uploads";
        public const string AVATAR_PATH = "uploads/avatars";
        public const string DOCUMENT_PATH = "uploads/documents";
        public const string REPORT_PATH = "uploads/reports";

        public static readonly string[] ALLOWED_IMAGE_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif" };
        public static readonly string[] ALLOWED_DOCUMENT_EXTENSIONS = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
    }
}
