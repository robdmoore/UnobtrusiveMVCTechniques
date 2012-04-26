using System.Web.Mvc;

namespace UnobtrusiveMVCTechniques.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
