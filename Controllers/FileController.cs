using FileShare.Attributes;
using FileShare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop.Implementation;
using System.Diagnostics;
using System.IO;

namespace FileShare.Controllers
{
    public class FileController : Controller
    {
        private const string filesPath = "/files/";
        private const int maxFileSize = 5 * 1024 * 1024;
        private readonly ILogger<FileController> _logger;
        private readonly IWebHostEnvironment _appEnvironment;

        public FileController(ILogger<FileController> logger, IWebHostEnvironment appEnvironment)
        {
            _logger = logger;
            this._appEnvironment = appEnvironment;
        }

        public ViewResult Index()
        {
            return View();
        }

        [HttpPost]
        [MaxFileSize(maxFileSize)]
        [AllowedExtensionsAttribute(new string[] { ".jpg", ".png" })]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                string path = filesPath + file.FileName;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                //FileModel fileModel = new FileModel { Name = file.FileName, Path = path };
                //_context.Files.Add(fileModel);
                //_context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Error();
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}