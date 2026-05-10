using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;

namespace MessageTest.Applibs
{
    internal class AutofacConfig
    {
        private static IContainer container;

        public static IContainer Container
        {
            get
            {
                if (container == null) Register();

                return container;
            }
        }

        public static void Register()
        {
            var builder = new ContainerBuilder();
            var asm = Assembly.GetExecutingAssembly();
            builder.RegisterApiControllers(asm);
        }
    }
}
