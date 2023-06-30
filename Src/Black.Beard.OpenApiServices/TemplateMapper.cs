using Bb.ComponentModel.Accessors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bb
{

    public static class TemplateMapper
    {

        public static string Map(this string template, object values)
        {

            var result = template;

            var type = values.GetType();
            var properties = PropertyAccessor.GetProperties(type, false);

            foreach (var property in properties)
            {
                var value = property.GetValue(values);
                result = result.Replace("{{" + property.Name + "}}", value.ToString(), false, CultureInfo.InvariantCulture);
            }

            return result;

        }


    }


}
