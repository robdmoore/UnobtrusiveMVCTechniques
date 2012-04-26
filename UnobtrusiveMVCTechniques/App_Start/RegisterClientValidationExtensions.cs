using DataAnnotationsExtensions.ClientValidation;

[assembly: WebActivator.PreApplicationStartMethod(typeof(UnobtrusiveMVCTechniques.App_Start.RegisterClientValidationExtensions), "Start")]
 
namespace UnobtrusiveMVCTechniques.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();            
        }
    }
}