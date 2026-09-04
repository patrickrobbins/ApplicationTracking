using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using DependencyTracker.Data.Repositories;
using DependencyTracker.Data.Services;
using DependencyTracker.Web.Models;

namespace DependencyTracker.Web.App_Start
{
    /// <summary>
    /// Configures Autofac as the MVC dependency resolver. Controllers, services,
    /// repositories, and the role provider are resolved per HTTP request.
    /// </summary>
    public static class AutofacConfig
    {
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.RegisterType<DevelopmentRoleProvider>()
                .Named<IRoleProvider>("Development")
                .InstancePerRequest();
            builder.RegisterType<ActiveDirectoryRoleProvider>()
                .Named<IRoleProvider>("ActiveDirectory")
                .InstancePerRequest();
            builder.Register(c =>
            {
                return RoleProviderFactory.IsDevelopment()
                    ? c.ResolveNamed<IRoleProvider>("Development")
                    : (IRoleProvider)c.ResolveNamed<IRoleProvider>("ActiveDirectory");
            })
                .As<IRoleProvider>()
                .InstancePerRequest();

            builder.RegisterType<ApplicationRepository>().As<IApplicationRepository>().InstancePerRequest();
            builder.RegisterType<ApplicationDllRepository>().As<IApplicationDllRepository>().InstancePerRequest();
            builder.RegisterType<DependencyRepository>().As<IDependencyRepository>().InstancePerRequest();
            builder.RegisterType<AdminRepository>().As<IAdminRepository>().InstancePerRequest();
            builder.RegisterType<ApplicationPropertyRepository>().As<IApplicationPropertyRepository>().InstancePerRequest();
            builder.RegisterType<ApplicationCategoryRepository>().As<IApplicationCategoryRepository>().InstancePerRequest();
            builder.RegisterType<ApplicationTechnologyRepository>().As<IApplicationTechnologyRepository>().InstancePerRequest();
            builder.RegisterType<ApplicationFamilyRepository>().As<IApplicationFamilyRepository>().InstancePerRequest();
            builder.RegisterType<TechnicalOwnershipTeamRepository>().As<ITechnicalOwnershipTeamRepository>().InstancePerRequest();
            builder.RegisterType<ApplicationTagRepository>().As<IApplicationTagRepository>().InstancePerRequest();

            builder.RegisterType<ApplicationService>().As<IApplicationService>().InstancePerRequest();
            builder.RegisterType<ApplicationDllService>().As<IApplicationDllService>().InstancePerRequest();
            builder.RegisterType<DependencyService>().As<IDependencyService>().InstancePerRequest();
            builder.RegisterType<AdminService>().As<IAdminService>().InstancePerRequest();
            builder.RegisterType<ApplicationPropertyService>().As<IApplicationPropertyService>().InstancePerRequest();
            builder.RegisterType<ApplicationCategoryService>().As<IApplicationCategoryService>().InstancePerRequest();
            builder.RegisterType<ApplicationTechnologyService>().As<IApplicationTechnologyService>().InstancePerRequest();
            builder.RegisterType<ApplicationFamilyService>().As<IApplicationFamilyService>().InstancePerRequest();
            builder.RegisterType<TechnicalOwnershipTeamService>().As<ITechnicalOwnershipTeamService>().InstancePerRequest();
            builder.RegisterType<ApplicationTagService>().As<IApplicationTagService>().InstancePerRequest();
            builder.RegisterType<ConfigScannerService>().As<IConfigScannerService>().InstancePerRequest();
            builder.RegisterType<AppDiscoveryService>().As<IAppDiscoveryService>().InstancePerRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
