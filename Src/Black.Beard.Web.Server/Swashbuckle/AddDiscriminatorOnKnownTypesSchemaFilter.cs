using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Bb.Swashbuckle
{

    /// <summary>
    /// Append discriminator on the specified property. The parameter give the name of the property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter" />
    public class AddDiscriminatorOnKnownTypesSchemaFilter<T> : ISchemaFilter
        where T : class
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AddDiscriminatorOnKnownTypesSchemaFilter{T}"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public AddDiscriminatorOnKnownTypesSchemaFilter(string propertyName)
        {
            this._type = typeof(T);
            _propertyName = propertyName;
            _propertyInfo = this._type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(c => c.Name == _propertyName && c.CanRead).FirstOrDefault();

        }

        /// <summary>
        /// Applies the specified schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {

            if (context.Type == this._type)
            {

                if (_propertyInfo == null)
                    throw new MissingMemberException("propertyName");

                var derivedTypes = typeof(T).Assembly.DefinedTypes
                    .Where(c => _type.IsAssignableFrom(c)
                        && c.IsClass
                        && !c.IsAbstract
                        && c.GetConstructor(_emptyTypeArray) != null
                        )
                    .ToList();

                var _dic = new Dictionary<string, string>();

                foreach (var derivedType in derivedTypes)
                {
                    var instance = Activator.CreateInstance(derivedType) as T;
                    if (instance != null)
                    {
                        string? propertySwitch = _propertyInfo.GetValue(instance)?.ToString();
                        if (propertySwitch != null)
                            _dic.Add(propertySwitch, derivedType.Name);
                    }
                }

                schema.Discriminator = new OpenApiDiscriminator { PropertyName = _propertyName, Mapping = _dic };

            }

        }

        private readonly Type _type;
        private readonly string _propertyName;
        private readonly PropertyInfo? _propertyInfo;
        private readonly Type[] _emptyTypeArray = new Type[0];

    }


}


