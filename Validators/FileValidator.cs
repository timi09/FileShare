using FileShare.Constants;
using FileShare.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FileShare.Validators;

public static class FileValidator
{
    public static void ValidateFile(IFormFile file)
    {
        if (file == null)
            throw new ValidationException("File is not selected");

        ValidateFileSize(file);
        ValidateFileExtension(file);
        ValidateFileName(file);
    }

    private static void ValidateFileSize(IFormFile file)
    {
        if (file.Length > FileValidationConstants.MaxFileSizeInBytes)
            throw new ValidationException($"Maximum allowed file size is {FileSizeHelper.GetFileSizeString(FileValidationConstants.MaxFileSizeInBytes)}. " +
                $"Size of {file.FileName} is {FileSizeHelper.GetFileSizeString(file.Length)}.");
    }

    private static void ValidateFileExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!FileValidationConstants.AllowedExtensions.Contains(extension.ToLower()))
            throw new ValidationException($"File extension \"{extension}\" is not allowed.");
    }

    private static void ValidateFileName(IFormFile file)
    {
        if (file.FileName.IndexOfAny(FileValidationConstants.ForbiddenChars) >= 0)
            throw new ValidationException("File name contains forbidden symbols.");
    }
}
