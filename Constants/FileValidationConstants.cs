namespace FileShare.Constants;

public class FileValidationConstants
{
    public const long MaxFileSizeInGb = 50;
    public const long MaxFileSizeInMb = MaxFileSizeInGb * 1024;
    public const long MaxFileSizeInBytes = MaxFileSizeInMb * 1024 * 1024;
    public static readonly string[] AllowedExtensions = { ".jpg", ".png", ".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar", ".drawio" };
    public static readonly string[] NotAllowedExtensions = { ".bat" };
    public static readonly char[] ForbiddenChars = "\\/:*?\"<>|".ToCharArray();
}
