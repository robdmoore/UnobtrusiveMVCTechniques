using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DataAnnotationsExtensions;

namespace UnobtrusiveMVCTechniques.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ValidationExample()
        {
            return View(new SomeViewModel());
        }

        [HttpPost]
        public ActionResult ValidationExample(SomeViewModel vm)
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

        #region DataAnnotationsExtensions
        [Email]
        public string Email { get; set; }

        [Url]
        public string Url { get; set; }
        #endregion
    }
}
