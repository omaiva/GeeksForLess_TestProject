using GeeksForLess_TestProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Net;

namespace GeeksForLess_TestProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHost;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHost)
        {
            _logger = logger;
            _webHost = webHost;
        }

        [HttpGet]
        public IActionResult Index()
        {
            string uploadsFolder = Path.Combine(_webHost.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "No file selected.";
                return View();
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (fileExtension != ".json")
            {
                ViewBag.Message = "Invalid file type. Please upload a .json file.";
                return View();
            }

            string uploadsFolder = Path.Combine(_webHost.WebRootPath,"uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            } else
            {
                Directory.Delete(uploadsFolder, true);
                Directory.CreateDirectory(uploadsFolder);
            }


            string fileName = Path.GetFileName(file.FileName);
            string fileSavePath = Path.Combine(uploadsFolder, fileName);

            using (FileStream stream = new FileStream(fileSavePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            ViewBag.Message = fileName + " uploaded succesfully";

            return View();
        }

        public IActionResult Tree()
        {
            string uploadsFolder = Path.Combine(_webHost.WebRootPath, "uploads");
            
            var webClient = new WebClient();
            string fileName = Directory.GetFiles(uploadsFolder).First();
            var json = webClient.DownloadString(fileName);
            DataTable dataTable = DataTableModel.ConvertToDataTable(json);

            DataBaseModel.UploadDataTableToSql(dataTable, "testTable", _webHost.WebRootPath);

            return View(dataTable);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
