using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using UnobtrusiveMVCTechniques.Repositories;

namespace UnobtrusiveMVCTechniques.Controllers
{
    public class CustomValidationController : Controller
    {
        #region Setup
        private readonly IUserRepository _userRepository;

        public CustomValidationController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        #endregion

        #region 1. Custom validation code in controller
        public ActionResult CustomCodeInController()
        {
            return View("ValidationTest");
        }

        [HttpPost]
        public ActionResult CustomCodeInController(ViewModel1 vm)
        {
            if (!ModelState.IsValid)
                return View("ValidationTest", vm);

            if (_userRepository.GetUserByUserName(vm.UserName) != null)
                ModelState.AddModelError("UserName", "That username is already taken; please try another username."); // In reality you would try not to use magic strings here

            if (!ModelState.IsValid)
                return View("ValidationTest", vm);

            ViewBag.Success = true;
            return View("ValidationTest");
        }
        public class ViewModel1 : TestViewModel { }
        #endregion

    }

    public class TestViewModel
    {
        [Required]
        public string UserName { get; set; }
    }
}
