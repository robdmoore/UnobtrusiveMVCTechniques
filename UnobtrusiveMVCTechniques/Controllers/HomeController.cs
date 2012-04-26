using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace UnobtrusiveMVCTechniques.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new SomeViewModel());
        }

        [HttpPost]
        public ActionResult Index(SomeViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            ViewBag.Success = true;
            return View(vm);
        }
    }

    public class SomeViewModel
    {
        #region Data type validation
        public int Integer { get; set; }
        #endregion

        #region System.ComponentModel.DataAnnotations
        [Required]
        public string Required { get; set; }

        [RegularExpression(@"\w")]
        public string RegexOneWordChar { get; set; }
        #endregion
    }
}
