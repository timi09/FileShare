using System.ComponentModel.DataAnnotations;

namespace FileShare.Attributes
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly HashSet<string> _extensions = new();
        public AllowedExtensionsAttribute(string[] extensions)
        {
            foreach (var extension in extensions)
                _extensions.Add(extension.ToLower());
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file == null)
                return new ValidationResult("File is not selected");

            var extension = Path.GetExtension(file.FileName);
            if (!_extensions.Contains(extension.ToLower()))
                return new ValidationResult("This extension is not allowed");

            return ValidationResult.Success;
        }
    }
}
