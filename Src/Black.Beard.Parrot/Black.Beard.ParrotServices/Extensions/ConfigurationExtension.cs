using Bb.ComponentModel.Attributes;
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

            _methodConfiguration = typeof(ConfigurationExtension).GetMethod(nameof(RegisterConfiguration), BindingFlags.Public | BindingFlags.Static);
            _methodModel = typeof(ConfigurationExtension).GetMethod(nameof(RegisterModel), BindingFlags.Public | BindingFlags.Static);
            _methodService = typeof(ConfigurationExtension).GetMethod(nameof(RegisterService), BindingFlags.Public | BindingFlags.Static);

        }


        //public static void UseServicesExposedByAttribute(this IServiceCollection services, IConfiguration configuration)
        //{

        //    foreach (var type in GetExposedTypes(Constants.Models.Service))
        //        _methodService.MakeGenericMethod(type).Invoke(null, new object[] { services, configuration });

        //}


        #region services

        public static void UseServicesExposedByAttribute(this IServiceCollection services, IConfiguration configuration)
        {

            foreach (var type in GetExposedTypes(Constants.Models.Service))
                _methodService.MakeGenericMethod(type).Invoke(null, new object[] { services, configuration });

        }

        public static void RegisterService<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class
        {

            if (typeof(T).IsAssignableFrom(typeof(IInitialize)))
            {

                Func<IServiceProvider, T> _func = (serviceProvider) =>
                {

                    var tService = serviceProvider.GetService<T>();

                    if (tService != null && tService is IInitialize s)
                        s.Initialize(serviceProvider, serviceProvider.GetService<IConfiguration>());

                    return tService;

                };

                services.RegisterType(_func);

            }
            else
            {
                services.RegisterType<T>();
            }

        }

        #endregion services

        #region models

        public static void UseModelsExposedByAttribute(this IServiceCollection services, IConfiguration configuration)
        {

            foreach (var type in GetExposedTypes(Constants.Models.Model))
                _methodModel.MakeGenericMethod(type).Invoke(null, new object[] { services, configuration });

        }

        public static void RegisterModel<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class
        {

            if (typeof(T).IsAssignableFrom(typeof(IInitialize)))
            {

                Func<IServiceProvider, T> _func = (serviceProvider) =>
                {

                    var tService = serviceProvider.GetService<T>();

                    if (tService != null && tService is IInitialize s)
                        s.Initialize(serviceProvider, serviceProvider.GetService<IConfiguration>());

                    return tService;

                };

                services.RegisterType(_func);


            }
            else
            {

                services.RegisterType<T>();

            }


        }

        #endregion models


        #region configuration

        public static void UseConfigurationsExposedByAttribute(this IServiceCollection services, IConfiguration configuration)
        {

            foreach (var type in GetExposedTypes(Constants.Models.Configuration))
                _methodModel.MakeGenericMethod(type).Invoke(null, new object[] { services, configuration });

        }

        public static void RegisterConfiguration<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class, new()
        {

            Func<IServiceProvider, T> _func = (serviceProvider) =>
            {
                var modelConfiguration = new T();
                configuration.Bind(nameof(T), modelConfiguration);
                return modelConfiguration;
            };

            services.RegisterType<T>(_func);

        }

        #endregion

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
        private static readonly MethodInfo? _methodConfiguration;
        private static readonly MethodInfo? _methodModel;
        private static readonly MethodInfo? _methodService;
    }

}
