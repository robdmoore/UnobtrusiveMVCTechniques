using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DataAnnotationsExtensions;
using FluentValidation;
using UnobtrusiveMVCTechniques.Repositories;

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

        #region Custom validation
        public string UserName { get; set; }
        public string UserNameFollowedByX { get; set; }
        #endregion
    }

    // This shouldn't normally go in the same file as the controller
    public class SomeViewModelValidator : AbstractValidator<SomeViewModel>
    {
        private readonly IUserRepository _userRepository;

        public SomeViewModelValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            RuleFor(x => x.UserName).Must(BeAUniqueUserName).WithMessage("That username is already taken; please try another username.");
            RuleFor(x => x.UserNameFollowedByX).Equal(s => s.UserName + "X").WithMessage("Please ensure this field is the username followed by the character 'X'.");
        }

        public bool BeAUniqueUserName(string userName)
        {
            return _userRepository.GetUserByUserName(userName) == null;
        }
    }
}
