using System.ComponentModel.DataAnnotations;

namespace FileShare.Attributes
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file == null)
                return new ValidationResult("File is not selected");

            if (file.Length > _maxFileSize)
                return new ValidationResult($"Maximum allowed file size is {_maxFileSize} bytes. " +
                    $"Sizes of {file.FileName} is {file.Length} bytes");

            return ValidationResult.Success;
        }
    }
}
