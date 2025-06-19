using System.Text.RegularExpressions;
using FileShare.Constants;
using FileShare.Data;
using FileShare.Helpers;
using FileShare.Interfaces;
using FileShare.Models;
using FileShare.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileShare.Controllers
{
    [Authorize]
    public class MyFilesController : Controller
    {
        private readonly ILogger<MyFilesController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserModel> _userManager;
        private readonly ILinkGenerateService _linkGenerateService;
        private readonly IFileStorageService _fileStorageService;

        public MyFilesController(ILogger<MyFilesController> logger,
            ApplicationDbContext context,
            UserManager<UserModel> userManager,
            ILinkGenerateService linkGenerateService,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _userManager = userManager;
            _linkGenerateService = linkGenerateService;
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        [Route("MyFiles")]
        public async Task<IActionResult> Index(string namePart, string sortParam)
        {
            try
            {
                ViewData["NamePart"] = namePart;
                ViewData["SortParam"] = sortParam;

                var userModel = await _userManager.GetUserAsync(User);
                var fileModels = _context.Files.Where(file => file.UserId == userModel.Id);

                if (!string.IsNullOrEmpty(namePart))
                {
                    fileModels = fileModels.Where(s => s.Name.ToLower().Contains(namePart.ToLower()));
                }
                switch (sortParam)
                {
                    case SortParamConstants.SizeAscending:
                        fileModels = fileModels.OrderBy(file => file.SizeInBytes);
                        break;
                    case SortParamConstants.SizeDescending:
                        fileModels = fileModels.OrderByDescending(file => file.SizeInBytes);
                        break;
                    case SortParamConstants.DateAscending:
                        fileModels = fileModels.OrderBy(file => file.UploadTime);
                        break;
                    case SortParamConstants.DateDescending:
                        fileModels = fileModels.OrderByDescending(file => file.UploadTime);
                        break;
                    default:
                        fileModels = fileModels.OrderByDescending(file => file.UploadTime);
                        break;
                }

                var fileViewModels = fileModels.Select(file => new FileViewModel
                {
                    Id = file.Id,
                    Name = file.Name,
                    UploadTime = file.UploadTime.ToLocalTime().ToString(DateTimeFormatConstants.DefaultDatetimeFormat),
                    Size = FileSizeHelper.GetFileSizeString(file.SizeInBytes)
                });

                return View(await fileViewModels.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem();
            }
        }

        [HttpGet]
        [Route("MyFiles/Download/{fileId}")]
        public async Task<IActionResult> Download(string fileId)
        {
            try
            {
                var userModel = await _userManager.GetUserAsync(User);
                var file = await _context.Files.FirstOrDefaultAsync(file => file.Id == fileId && file.UserId == userModel.Id);

                if (file == null)
                    return NotFound("File not found");

                var fileBytes = await _fileStorageService.ReadFileAsBytesAsync(fileId);

                return File(fileBytes, "application/octet-stream", file.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem();
            }
        }

        [HttpGet]
        [Route("MyFiles/GenerateShortLink")]
        public async Task<IActionResult> GenerateShortLink(string fileId, int length, int? maxDownloads, bool unlimited)
        {
            var userModel = await _userManager.GetUserAsync(User);
            var file = await _context.Files.FirstOrDefaultAsync(file => file.Id == fileId && file.UserId == userModel.Id);

            if (file == null)
                return NotFound("File not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldLink = await _context.Links.FirstOrDefaultAsync(link => link.FileId == fileId);

                if (oldLink != null)
                    _context.Links.Remove(oldLink);

                var linkIdLength = length - (Request.Host.Value.Length + 1);

                LinkModel newLink = null;
                do
                {
                    newLink = _linkGenerateService.GenerateLink(file, linkIdLength);
                }
                while (await _context.Links.AnyAsync(link => link.Id == newLink.Id));

                _context.Links.Add(newLink);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    ShortUrl = $"{Request.Host}/{newLink.Id}",
                    Max = newLink.MaxDownloadCount,
                    Current = newLink.CurrentDownloadCount,
                    Unlimited = newLink.Unlimited
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, ex.Message);
                return Problem();
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadByShortLink(string linkId)
        {
            // Защита от конфликтов с обычными маршрутами (MyFiles, Account и т.п.)
            if (!Regex.IsMatch(linkId, "^[a-zA-Z0-9]{3,23}$"))
                return NotFound();

            var link = await _context.Links.Include(link => link.File).FirstOrDefaultAsync(link => link.Id == linkId);

            if (link == null)
                return NotFound("Link not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                link.CurrentDownloadCount++;

                if(!link.Unlimited && link.CurrentDownloadCount >= link.MaxDownloadCount)
                    _context.Links.Remove(link);

                await _context.SaveChangesAsync();

                var fileBytes = await _fileStorageService.ReadFileAsBytesAsync(link.FileId);

                await transaction.CommitAsync();

                return File(fileBytes, "application/x-freearc", link.File.Name);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, ex.Message);
                return Problem();
            }
        }

        [HttpPost]
        [Route("MyFiles/Delete")]
        public async Task<IActionResult> Delete(string fileId)
        {
            var userModel = await _userManager.GetUserAsync(User);
            var file = await _context.Files.FirstOrDefaultAsync(file => file.Id == fileId && file.UserId == userModel.Id);

            if (file == null)
                return NotFound("File not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Files.Remove(file);
                _fileStorageService.DeleteFile(fileId);
                
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("MyFiles/GetShortLink")]
        public async Task<IActionResult> GetShortLinkInfo(string fileId)
        {
            var userModel = await _userManager.GetUserAsync(User);
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userModel.Id);

            if (file == null)
                return NotFound("File not found");

            var link = await _context.Links.FirstOrDefaultAsync(l => l.FileId == fileId);

            if (link == null)
                return NotFound(null); // нет ссылки

            return Ok(new
            {
                ShortUrl = $"{Request.Host}/{link.Id}",
                Current = link.CurrentDownloadCount,
                Max = link.MaxDownloadCount,
                Unlimited = link.Unlimited
            });
        }

        [HttpPost]
        [Route("MyFiles/UpdateShortLink")]
        public async Task<IActionResult> UpdateShortLink(string fileId, int? maxDownloads, bool unlimited)
        {
            var userModel = await _userManager.GetUserAsync(User);
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userModel.Id);

            if (file == null)
                return NotFound("File not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldLink = await _context.Links.FirstOrDefaultAsync(l => l.FileId == fileId);

                oldLink.MaxDownloadCount = maxDownloads ?? 1;
                oldLink.Unlimited = unlimited;

                _context.Links.Update(oldLink);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    ShortUrl = $"{Request.Host}/{oldLink.Id}",
                    Max = oldLink.MaxDownloadCount,
                    Current = oldLink.CurrentDownloadCount,
                    Unlimited = oldLink.Unlimited
                });
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
