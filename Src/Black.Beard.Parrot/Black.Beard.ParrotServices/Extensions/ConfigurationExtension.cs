using Bb.ComponentModel.Attributes;
using Bb.Expressions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bb.Extensions
{

    public static class ConfigurationExtension
    {

        static ConfigurationExtension()
        {

            _method = typeof(ConfigurationExtension).GetMethod(nameof(RegisterConfig),
                  BindingFlags.Public
                | BindingFlags.Static);

        }

        public static void UseConfigurationByAttribute(this IServiceCollection services, IConfiguration configuration)
        {

            foreach (var type in GetConfigurationTypes())
            {
                var attribute = type.GetCustomAttribute<ExposeClassAttribute>();
                MethodInfo method = _method.MakeGenericMethod(type);
                method.Invoke(null, new object[] { services, configuration, attribute });
            }

        }

        public static void RegisterConfig<T>(this IServiceCollection services, IConfiguration configuration, ExposeClassAttribute attribute)
            where T : class, new()
        {

            var exposed = typeof(T);

            Func<IServiceProvider, T> _func = (serviceProvider) =>
            {
                var modelConfiguration = new T();
                configuration.Bind(nameof(T), modelConfiguration);
                return modelConfiguration;
            };

            switch (attribute.LifeCycle)
            {

                case IocScopeEnum.Transiant:
                    services.AddTransient(exposed, _func);
                    break;

                case IocScopeEnum.Scoped:
                    services.AddScoped(exposed, _func);
                    break;

                case IocScopeEnum.Singleton:
                default:
                    if (attribute.ExposedType != null)
                        exposed = attribute.ExposedType;
                    services.AddSingleton(exposed, _func);
                    break;
            }


        }


        public static IEnumerable<Type> GetConfigurationTypes()
        {
            var items = Bb.ComponentModel.TypeDiscovery.Instance.GetTypesWithAttributes<ExposeClassAttribute>(typeof(object), c => c.Context == "Configuration");
            return items;
        }


        private static readonly MethodInfo? _method;

    }

    public class Constants
    {

        public static class Models
        {
            public const string Configuration = "Configuration";
        }

    }

}
