using FileShare.Data;
using FileShare.Interfaces;
using FileShare.Models;
using FileShare.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FileShare.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly ILogger<UploadController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;
        private readonly IFileStorageService _fileStorageService;

        public UploadController(ILogger<UploadController> logger,
            ApplicationDbContext context,
            UserManager<UserModel> userManager,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _fileStorageService = fileStorageService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                FileValidator.ValidateFile(file);

                var userModel = await _userManager.GetUserAsync(User);

                var fileModel = new FileModel
                {
                    UserId = userModel.Id,
                    Name = file.FileName,
                    UploadTime = DateTime.UtcNow,
                    SizeInBytes = file.Length
                };

                _context.Add(fileModel);

                await _context.SaveChangesAsync();

                await _fileStorageService.SaveFileAsync(file, fileModel.Id);

                await transaction.CommitAsync();

                return Ok();
            }
            catch (ValidationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, ex.Message);
                return Problem();
            }
        }
    }
}
