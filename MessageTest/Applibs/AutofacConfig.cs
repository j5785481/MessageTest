using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;
using MessageTest.Domain.Repository;

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

            // sql ioc
            builder.RegisterAssemblyTypes(Assembly.Load("MessageTest.Persistent"), Assembly.Load("MessageTest.Domain"))
                .WithParameter("connectionString", ConfigHelper.ConnectionString)
                .Where(t => t.Namespace == "MessageTest.Persistent.Sql" || t.Namespace == "MessageTest.Domain.Repository")
                .AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();

            container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                // 如果這行噴錯，就代表 ISubjectPoRepository 沒註冊成功
                var test = scope.Resolve<ISubjectPoRepository>();
            }
        }
    }
}
