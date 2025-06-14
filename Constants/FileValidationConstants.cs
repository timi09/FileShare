namespace FileShare.Constants;

public class FileValidationConstants
{
    public const int MaxFileSizeInMb = 100;
    public const int MaxFileSizeInBytes = MaxFileSizeInMb * 1024 * 1024;
    public static readonly string[] AllowedExtensions = { ".jpg", ".png", ".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar", ".drawio" };
    public static readonly char[] ForbiddenChars = "\\/:*?\"<>|".ToCharArray();
}
