using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac;
using FluentValidation;
using Module = Autofac.Module;

namespace UnobtrusiveMVCTechniques.Config.Autofac
{
    public class FluentValidatorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<AutofacValidatorFactory>().As<IValidatorFactory>().SingleInstance();

            var validators = AssemblyScanner.FindValidatorsInAssembly(Assembly.GetExecutingAssembly());
            validators.ToList().ForEach(v => builder.RegisterType(v.ValidatorType).As(v.InterfaceType).InstancePerDependency());
        }
    }

    public class AutofacValidatorFactory : IValidatorFactory
    {
        public IValidator<T> GetValidator<T>()
        {
            return (IValidator<T>)GetValidator(typeof(T));
        }

        public IValidator GetValidator(Type type)
        {
            var genericType = typeof(IValidator<>).MakeGenericType(type);
            try
            {
                return (IValidator)DependencyResolver.Current.GetService(genericType);
            }
            catch (Exception) { }

            return null;
        }
    }
}