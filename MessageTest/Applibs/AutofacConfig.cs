using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;
using ForumMessageSystem.Persistent.Core;
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

            // mongo ioc
            builder.RegisterAssemblyTypes(Assembly.Load("MessageTest.Domain"),
                    Assembly.Load("MessageTest.Persistent"))
                .Where(t => t.IsAssignableTo<IRepository>() && t.IsAssignableTo<BaseMongoRepository>())
                .WithParameter("mongoClient", NoSqlService.MongoConnetion)
                .WithParameter("dataBaseName", "MessageTest")
                .As(t => t.GetInterfaces().FirstOrDefault(i => i.Name == $"I{t.Name}"))
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
