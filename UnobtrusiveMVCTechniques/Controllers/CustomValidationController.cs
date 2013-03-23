using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac;
using FluentValidation;
using UnobtrusiveMVCTechniques.Repositories;
using Module = Autofac.Module;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using Validator = FluentValidation.Attributes.ValidatorAttribute;

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

            // Yuck!
            if (_userRepository.GetUserByUserName(vm.UserName) != null)
                ModelState.AddModelError("UserName", "That username is already taken; please try another username."); // In reality you would try not to use magic strings here

            // Even worse!
            if (!ModelState.IsValid)
                return View("ValidationTest", vm);

            ViewBag.Success = true;
            return View("ValidationTest");
        }
        public class ViewModel1 : TestViewModel { }
        #endregion

        #region 2. Modularised custom validation code called from controller
        public ActionResult ModularisedCustomCodeCalledFromController()
        {
            return View("ValidationTest");
        }

        [HttpPost]
        public ActionResult ModularisedCustomCodeCalledFromController(ViewModel2 vm)
        {
            // Much better, but still gross
            if (!ModelState.IsValid || !vm.Validate(ModelState, _userRepository))
                return View("ValidationTest", vm);

            ViewBag.Success = true;
            return View("ValidationTest");
        }
        public class ViewModel2 : TestViewModel
        {
            public bool Validate(ModelStateDictionary modelState, IUserRepository userRepository)
            {
                if (userRepository.GetUserByUserName(UserName) != null)
                    modelState.AddModelError("UserName", "That username is already taken; please try another username."); // In reality you would try not to use magic strings here

                return modelState.IsValid;
            }
        }
        #endregion

        #region 3. Get default model binder to call the validation code before the controller action
        public ActionResult ValidationAlreadyCalled()
        {
            return View("ValidationTest");
        }

        [HttpPost]
        public ActionResult ValidationAlreadyCalled(ViewModel3 vm)
        {
            // Much better!
            if (!ModelState.IsValid)
                return View("ValidationTest", vm);

            ViewBag.Success = true;
            return View("ValidationTest");
        }
        public class ViewModel3 : TestViewModel, IValidatableObject
        {
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                var userRepository = DependencyResolver.Current.GetService<IUserRepository>(); // Yuck!
                if (userRepository.GetUserByUserName(UserName) != null)
                    yield return new ValidationResult("That username is already taken; please try another username.", new [] {"UserName"});
            }
        }
        #endregion

        #region 4. Introduce a validation library
        public ActionResult FluentValidation()
        {
            return View("ValidationTest");
        }

        [HttpPost]
        public ActionResult FluentValidation(ViewModel4 vm)
        {
            if (!ModelState.IsValid)
                return View("ValidationTest", vm);

            ViewBag.Success = true;
            return View("ValidationTest");
        }
        public class ViewModel4 : TestViewModel, IValidatableObject
        {
            // Ugh! A whole heap of extra code we have to write for each View Model!
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                var validationResult = new ViewModel4Validator().Validate(this); // Yuck!
                if (!validationResult.IsValid)
                {
                    return validationResult.Errors.ToList().Select(e => new ValidationResult(e.ErrorMessage, new[] { e.PropertyName }));
                }
                return new List<ValidationResult>();
            }
        }
        public class ViewModel4Validator : AbstractValidator<ViewModel4>
        {
            public ViewModel4Validator()
            {
                // Much more expressive!
                // No magic string for the property name - sweet!
                RuleFor(x => x.UserName).Must(BeAUniqueUserName).WithMessage("That username is already taken; please try another username.");
            }

            public bool BeAUniqueUserName(string userName)
            {
                var userRepository = DependencyResolver.Current.GetService<IUserRepository>(); // Yuck!
                return userRepository.GetUserByUserName(userName) == null;
            }
        }
        #endregion

        #region 5. Use ModelValidatorProvider
        // Note: Requires FluentValidationModelValidatorProvider.Configure(); in Application_Start() in Global.asax.cs
        public ActionResult ModelValidatorProvider()
        {
            return View("ValidationTest");
        }

        [HttpPost]
        public ActionResult ModelValidatorProvider(ViewModel5 vm)
        {
            if (!ModelState.IsValid)
                return View("ValidationTest", vm);

            ViewBag.Success = true;
            return View("ValidationTest");
        }
        [Validator(typeof(ViewModel5Validator))] // Not too bad, but we could find this out with Reflection given the type of the validator below
        public class ViewModel5 : TestViewModel {}
        public class ViewModel5Validator : AbstractValidator<ViewModel5>
        {
            public ViewModel5Validator()
            {
                RuleFor(x => x.UserName).Must(BeAUniqueUserName).WithMessage("That username is already taken; please try another username.");
            }

            public bool BeAUniqueUserName(string userName)
            {
                var userRepository = DependencyResolver.Current.GetService<IUserRepository>(); // Yuck!
                return userRepository.GetUserByUserName(userName) == null;
            }
        }
        #endregion

        #region 6. ModelValidatorProvider + DI magic
        #region Once-off setup, obviously this shouldn't normally go into the controller
        // Note: Requires builder.RegisterModule<FluentValidatorModule>();
        //  and ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(container.Resolve<IValidatorFactory>()) { AddImplicitRequiredValidator = false });
        //  in Application_Start within Global.asax.cs
        public class AutofacValidatorFactory : IValidatorFactory
        {
            public IValidator<T> GetValidator<T>()
            {
                return (IValidator<T>) GetValidator(typeof(T));
            }

            public IValidator GetValidator(Type type)
            {
                var genericType = typeof(IValidator<>).MakeGenericType(type);
                try
                {
                    return (IValidator) DependencyResolver.Current.GetService(genericType);
                }
                catch (Exception) { }

                return null;
            }
        }
        public class FluentValidatorModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);
                builder.RegisterType<AutofacValidatorFactory>().As<IValidatorFactory>().SingleInstance();

                var validators = AssemblyScanner.FindValidatorsInAssembly(Assembly.GetExecutingAssembly());
                // There are three different options for the scoping of the next line of code; see https://github.com/robdmoore/UnobtrusiveMVCTechniques/issues/1
                validators.ToList().ForEach(v => builder.RegisterType(v.ValidatorType).As(v.InterfaceType).InstancePerDependency());
            }
        }
        #endregion
        public ActionResult ModelValidatorProviderWithDi()
        {
            return View("ValidationTest");
        }

        [HttpPost]
        public ActionResult ModelValidatorProviderWithDi(ViewModel6 vm)
        {
            if (!ModelState.IsValid)
                return View("ValidationTest", vm);

            ViewBag.Success = true;
            return View("ValidationTest");
        }
        public class ViewModel6 : TestViewModel { }
        public class ViewModel6Validator : AbstractValidator<ViewModel6>
        {
            private readonly IUserRepository _userRepository;

            public ViewModel6Validator(IUserRepository userRepository)
            {
                _userRepository = userRepository; // Woot! DI FTW, so much easier to test this validator now
                RuleFor(x => x.UserName).Must(BeAUniqueUserName).WithMessage("That username is already taken; please try another username.");
            }

            public bool BeAUniqueUserName(string userName)
            {
                return _userRepository.GetUserByUserName(userName) == null;
            }
        }
        #endregion
    }

    public class TestViewModel
    {
        [Required]
        public string UserName { get; set; }
    }
}
