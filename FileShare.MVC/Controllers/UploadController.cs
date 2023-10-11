using FileShare.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FileShare.Controllers
{
    public class UploadController : Controller
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<UploadController> _logger;
        private readonly IWebHostEnvironment _appEnvironment;

        public UploadController(IFileRepository fileRepository, ILogger<UploadController> logger, IWebHostEnvironment appEnvironment)
        {
            _fileRepository = fileRepository;
            _logger = logger;
            _appEnvironment = appEnvironment;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                ValidateFile(file);

                Entities.File fileEntity = new Entities.File
                {
                    Name = file.FileName,
                    Time = DateTime.Now,
                    Size = file.Length
                };

                _fileRepository.SaveFile(fileEntity);

                var filePath = Path.Combine(_appEnvironment.WebRootPath, "files", file.FileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return Ok();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem();
            }
        }

        private const int maxFileSize = 5 * 1024 * 1024;

        private readonly HashSet<string> extensions = new() { ".jpg", ".png", ".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx", };

        private static readonly char[] chars = "\\/:*?\"<>|".ToCharArray();

        private void ValidateFile(IFormFile file) 
        {
            if (file == null)
                throw new ValidationException("File is not selected");

            if (file.Length > maxFileSize)
                throw new ValidationException($"Maximum allowed file size is {maxFileSize} bytes. " +
                    $"Sizes of {file.FileName} is {file.Length} bytes");

            var extension = Path.GetExtension(file.FileName);
            if (!extensions.Contains(extension.ToLower()))
                throw new ValidationException("This extension is not allowed");

            if (file.FileName.IndexOfAny(chars) >= 0)
                throw new ValidationException("File name contains forbidden symbols");
        }
    }
}
