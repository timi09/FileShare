using FileShare.Core.Services;
using FileShare.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FileShare.Controllers
{
    public class DownloadController : Controller
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILinkRepository _linkRepository;
        private readonly ILinkGenerator _linkGenerator;
        private readonly ILogger<DownloadController> _logger;
        private readonly IWebHostEnvironment _appEnvironment;

        public DownloadController(IFileRepository fileRepository, ILinkRepository linkRepository, 
            ILinkGenerator linkGenerator, IWebHostEnvironment appEnvironment, ILogger<DownloadController> logger)
        {
            _fileRepository = fileRepository;
            _linkRepository = linkRepository;
            _linkGenerator = linkGenerator;
            _appEnvironment = appEnvironment;
            _logger = logger;
        }

        public ActionResult Index()
        {
            return View(_fileRepository.GetAllFiles());
        }

        [HttpGet]
        [Route("Download/{fileId?}")]
        public async Task<IActionResult> Download(int fileId)
        {
            try
            {
                var file = _fileRepository.GetFile(fileId);

                if (file == null)
                    return NotFound("File not found");

                var filePath = Path.Combine(_appEnvironment.WebRootPath, "files", file.Name);

                return File(System.IO.File.ReadAllBytes(filePath), "application/octet-stream", file.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem();
            }
        }

        [HttpGet]
        [Route("/GenerateShortLink")]
        public async Task<IActionResult> GenerateShortLink(int fileId)
        {
            try
            {
                var file = _fileRepository.GetFile(fileId);

                if (file == null)
                    return NotFound("File not found");

                var link = _linkRepository.GetLinkByFile(fileId);

                if (link != null)
                    _linkRepository.DeleteLink(link.Id);

                link = _linkGenerator.GenerateLink(file);

                _linkRepository.SaveLink(link);

                string domainName = Request.Host.ToString();

                return Ok($"https://{domainName}/dwld/{link.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem();
            }
        }

        [HttpGet]
        [Route("dwld/{linkId?}")]
        public async Task<IActionResult> DownloadByShortLink(string linkId)
        {
            try
            {
                var link = _linkRepository.GetLink(linkId);

                if (link == null)
                    return NotFound("File not found");

                _linkRepository.DeleteLink(link.Id);

                var filePath = Path.Combine(_appEnvironment.WebRootPath, "files", link.File.Name);

                return File(System.IO.File.ReadAllBytes(filePath), "application/x-freearc", link.File.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem();
            }
        }
    }
}
