using Bb.ComponentModel.Attributes;
using Bb.ComponentModel.Factories;
using Bb.Expressions;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System.Reflection;

namespace Bb.Extensions
{

    public static class ConfigurationExtension
    {

        static ConfigurationExtension()
        {
            _logger = LogManager.GetLogger(nameof(ConfigurationExtension));
            _method = typeof(ConfigurationExtension).GetMethod(nameof(AddType), BindingFlags.NonPublic | BindingFlags.Static);
        }


        public static IServiceCollection UseTypeExposedByAttribute(this IServiceCollection services, IConfiguration configuration, string contextKey, Action<Type> action = null)
        {
            foreach (var type in GetExposedTypes(Constants.Models.Service))
            {
                
                _method.MakeGenericMethod(type).Invoke(null, new object[] { services, configuration });

                if (action != null)
                    action(type);

            }

            return services;

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
        private static readonly MethodInfo? _method;
        private static readonly MethodInfo? _methodConfiguration;
        private static readonly MethodInfo? _methodModel;
        private static readonly MethodInfo? _methodService;
    }

}
