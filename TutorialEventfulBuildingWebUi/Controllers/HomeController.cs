using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace TutorialEventfulBuildingWebUi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public FileResult RetornoArquivoIFC(string file)
        {
            var path = Path.Combine(HostingEnvironment.ApplicationPhysicalPath,@"FileWexBIMTests\", file+".wexBIM");
            var fileStream = System.IO.File.OpenRead(path);
            
            return File(fileStream, System.Net.Mime.MediaTypeNames.Application.Octet);
        }
    }
}