using Bb.ComponentModel.Attributes;
using Bb.ComponentModel.Factories;
using NLog;
using System.Reflection;

namespace Bb.Extensions
{

    public static class IocAutoDiscoverExtension
    {

        static IocAutoDiscoverExtension()
        {
            _logger = LogManager.GetLogger(nameof(IocAutoDiscoverExtension));
            _methodRegister = typeof(IocAutoDiscoverExtension).GetMethod(nameof(AddType), BindingFlags.NonPublic | BindingFlags.Static);
            _methodOptionConfiguration = typeof(IocAutoDiscoverExtension).GetMethod(nameof(BindConfiguration), BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static IServiceCollection BindConfiguration(this IServiceCollection self, Type type, IConfiguration configuration)
        {


            _methodOptionConfiguration.MakeGenericMethod(type)
                .Invoke(self, new object[] { self, configuration });

            return self;

        }


        public static IServiceCollection UseTypeExposedByAttribute(this IServiceCollection services, IConfiguration configuration, string contextKey, Action<Type> action = null)
        {
            foreach (var type in GetExposedTypes(contextKey))
            {

                _methodRegister.MakeGenericMethod(type).Invoke(null, new object[] { services, configuration });

                if (action != null)
                    action(type);

            }

            return services;

        }




        private static void BindConfiguration<TOptions>(this IServiceCollection self, IConfiguration configuration)
            where TOptions : class
        {
            var attribute = typeof(TOptions).GetCustomAttribute<ExposeClassAttribute>();
            var sectionName = !string.IsNullOrEmpty(attribute?.Name) ? attribute.Name : typeof(TOptions).Name;
            var section = configuration.GetSection(sectionName);
            self.AddOptions<TOptions>().Bind(section).ValidateDataAnnotations();

        }
        
        private static void AddType<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class
        {
            if (typeof(IInitialize).IsAssignableFrom(typeof(T)))
            {
                var serviceBuilder = ObjectCreatorByIoc.GetActivator<T>();
                Func<IServiceProvider, T> _func = (serviceProvider) => serviceBuilder.Call(null, serviceProvider);
                services.RegisterType(_func);
            }
            else
                services.RegisterType<T>();
        }

        private static void RegisterType<T>(this IServiceCollection services, Func<IServiceProvider, T> func)
            where T : class
        {

            var attribute = typeof(T).GetCustomAttribute<ExposeClassAttribute>();
            var exposed = attribute?.ExposedType ?? typeof(T);

            switch (attribute.LifeCycle)
            {

                case IocScopeEnum.Transiant:
                    services.AddTransient(exposed, func);
                    break;

                case IocScopeEnum.Scoped:
                    services.AddScoped(exposed, func);
                    break;

                case IocScopeEnum.Singleton:
                default:
                    if (attribute.ExposedType != null)
                        exposed = attribute.ExposedType;
                    services.AddSingleton(exposed, func);
                    break;
            }

            _logger.Debug("registered {contextModel} {type} exposed by {exposed} with lifecycle {lifeCycle}", attribute.Context, typeof(T).Name, exposed.Name, attribute.LifeCycle.ToString());

        }

        private static void RegisterType<T>(this IServiceCollection services)
            where T : class
        {

            var attribute = typeof(T).GetCustomAttribute<ExposeClassAttribute>();
            var exposed = attribute?.ExposedType ?? typeof(T);

            switch (attribute.LifeCycle)
            {

                case IocScopeEnum.Singleton:
                    if (attribute.ExposedType != null)
                        exposed = attribute.ExposedType;
                    services.AddSingleton(exposed, typeof(T));
                    break;

                case IocScopeEnum.Scoped:
                    services.AddScoped(exposed, typeof(T));
                    break;

                case IocScopeEnum.Transiant:
                default:
                    services.AddTransient(exposed, typeof(T));
                    break;

            }

            _logger.Debug("registered {contextModel} {type} exposed by {exposed} with lifecycle {lifeCycle}", attribute.Context, typeof(T).Name, exposed.Name, attribute.LifeCycle.ToString());

        }


        /// <summary>
        /// Gets the exposed types in loaded assemblies.
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetExposedTypes(string contextName)
        {
            var items = ComponentModel.TypeDiscovery.Instance.GetTypesWithAttributes<ExposeClassAttribute>(typeof(object), c => c.Context == contextName);
            return items;
        }


        private static readonly Logger _logger;
        private static readonly MethodInfo? _methodRegister;
        private static readonly MethodInfo? _methodOptionConfiguration;
        private static readonly MethodInfo? _methodConfiguration;
        private static readonly MethodInfo? _methodModel;
        private static readonly MethodInfo? _methodService;

    }

}
