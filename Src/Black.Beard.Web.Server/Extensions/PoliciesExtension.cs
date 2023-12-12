using Bb.Models.Security;
using Flurl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace Bb.Extensions
{

    public static class PoliciesExtension
    {

        /// <summary>
        /// Initializes the <see cref="PoliciesExtension"/> class.
        /// </summary>
        static PoliciesExtension()
        {
            _controllerBaseType = typeof(ControllerBase);
            _match = "[Controller]";
        }


        /// <summary>
        /// Saves the datas policies in specified path.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="path">The path.</param>
        public static void SaveInFolder(this List<PolicyModel> self, string path)
        {

            string filename = Path.Combine(path, "policies.config.json");

            PolicyProfilList
                .New(self)
                .Save(filename);

        }

        /// <summary>
        /// Saves the datas policies in specified path.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="filename">The path.</param>
        public static void Save(this List<PolicyModel> self, string filename)
        {

            PolicyProfilList
                .New(self)
                .Save(filename);

        }

        public static List<PolicyModel> GetPolicies() => PoliciesExtension.GetTypes().GetTypePolicies().ToList();

        /// <summary>
        /// Return the list of method for all declarated controller
        /// For every method the Autorizes strategy is identified
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        private static IEnumerable<PolicyModel> GetTypePolicies(this IEnumerable<Type> self)
        {

            Dictionary<string, PolicyModel> _doc = new Dictionary<string, PolicyModel>();

            foreach (var type in self)
            {

                var _routes = GetRoots(type);
                var _roles = GetRoles(type);
                var _schemes = GetAuthenticationSchemes(type);

                if (_routes.Count == 0)
                    _routes.Add(string.Empty);

                var autorizeAttributes1 = type.GetCustomAttributes(typeof(AuthorizeAttribute), true).OfType<AuthorizeAttribute>().ToArray();

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (var method in methods)
                {

                    var m = method.GetCustomAttributes()
                        .Where(c => typeof(HttpMethodAttribute).IsAssignableFrom(c.GetType()))
                        .OfType<HttpMethodAttribute>().ToArray();

                    if (m.Any())
                    {

                        foreach (var policyName in GetPolicies(autorizeAttributes1))
                        {

                            if (!_doc.TryGetValue(policyName, out var model))
                                _doc.Add(policyName, model = new PolicyModel() { Name = policyName });

                            foreach (var route in BuildRoute(_routes, method))
                                if (!model.Routes.TryGetValue(route, out var routeModel))
                                    model.Routes.Add(route, routeModel = new PolicyModelRoute(type) { Route = route });

                            foreach (var route in BuildRoute(_routes, m))
                                if (!model.Routes.TryGetValue(route, out var routeModel))
                                    model.Routes.Add(route, routeModel = new PolicyModelRoute(type) { Route = route });

                        }

                        var v2 = method.GetCustomAttributes(typeof(AuthorizeAttribute), true).OfType<AuthorizeAttribute>().ToArray();
                        foreach (var policyName in GetPolicies(v2))
                        {

                            if (!_doc.TryGetValue(policyName, out var model))
                                _doc.Add(policyName, model = new PolicyModel() { Name = policyName });

                            foreach (var route in BuildRoute(_routes, method))
                                if (!model.Routes.TryGetValue(route, out var routeModel))
                                    model.Routes.Add(route, routeModel = new PolicyModelRoute(type) { Route = route });

                            foreach (var route in BuildRoute(_routes, m))
                                if (!model.Routes.TryGetValue(route, out var routeModel))
                                    model.Routes.Add(route, routeModel = new PolicyModelRoute(type) { Route = route });

                        }

                    }
                }

            }

            foreach (var item in _doc.Values)
                yield return item;

        }

        private static IEnumerable<string> GetPolicies(this AuthorizeAttribute[] autorizes)
        {

            if (autorizes != null)
                foreach (var autorize in autorizes)
                    if (autorize != null)
                        if (!string.IsNullOrEmpty(autorize.Policy))
                        {
                            foreach (string policy in autorize.Policy.Trim().Split(','))
                            {
                                var r = policy.Trim();
                                if (!string.IsNullOrEmpty(r))
                                    yield return r;
                            }
                        }

        }

        private static HashSet<string> BuildRoute(HashSet<string> routePaths, MethodInfo method)
        {

            var _routes = new HashSet<string>();

            var attr2 = method.GetCustomAttributes(typeof(RouteAttribute), true).OfType<RouteAttribute>().ToArray();
            if (attr2 != null)
                foreach (var routeAttribute in attr2)
                    if (routeAttribute != null)
                    {
                        var t = routeAttribute.Template;
                        foreach (var item in routePaths)
                        {
                            var r = Url.Combine(item, t).ToString().Replace("%7B", "{").Replace("%7D", "}");
                            _routes.Add(r);
                        }
                    }

            return _routes;

        }

        private static HashSet<string> BuildRoute(HashSet<string> routePaths, HttpMethodAttribute[] attr2)
        {

            var _routes = new HashSet<string>();

            if (attr2 != null)
                foreach (var routeAttribute in attr2)
                    if (routeAttribute != null)
                    {
                        var t = routeAttribute.Template;
                        foreach (var item in routePaths)
                        {
                            var r = Url.Combine(item, t).ToString().Replace("%7B", "{").Replace("%7D", "}");
                            _routes.Add(r);
                        }
                    }

            return _routes;

        }

        private static HashSet<string> GetRoots(Type type)
        {
            var _items = new HashSet<string>();
            var controllerName = type.Name.Substring(0, type.Name.Length - "Controller".Length);
            var routeAttributes1 = type.GetCustomAttributes(typeof(RouteAttribute), true).OfType<RouteAttribute>().ToArray();
            if (routeAttributes1 != null)
                foreach (var routeAttribute in routeAttributes1)
                    if (routeAttribute != null)
                        _items.Add(routeAttribute.Template.Replace(_match, controllerName, StringComparison.InvariantCultureIgnoreCase));

            return _items;

        }

        private static HashSet<string> GetRoles(Type type)
        {
            var _items = new HashSet<string>();
            var controllerName = type.Name.Substring(0, type.Name.Length - "Controller".Length);
            var autorizes = type.GetCustomAttributes(typeof(AuthorizeAttribute), true).OfType<AuthorizeAttribute>().ToArray();
            if (autorizes != null)
                foreach (var authorize in autorizes)
                    if (authorize != null)
                    {

                        if (!string.IsNullOrEmpty(authorize.Roles))
                            foreach (string role in authorize.Roles.Trim().Split(','))
                            {
                                var r = role.Trim();
                                if (!string.IsNullOrEmpty(r))
                                    _items.Add(r);
                            }

                    }

            return _items;

        }

        private static HashSet<string> GetAuthenticationSchemes(Type type)
        {
            var _items = new HashSet<string>();
            var controllerName = type.Name.Substring(0, type.Name.Length - "Controller".Length);
            var autorizes = type.GetCustomAttributes(typeof(AuthorizeAttribute), true).OfType<AuthorizeAttribute>().ToArray();
            if (autorizes != null)
                foreach (var authorize in autorizes)
                    if (authorize != null)
                    {

                        if (!string.IsNullOrEmpty(authorize.AuthenticationSchemes))
                            foreach (string scheme in authorize.AuthenticationSchemes.Trim().Split(','))
                            {
                                var r = scheme.Trim();
                                if (!string.IsNullOrEmpty(r))
                                    _items.Add(r);
                            }

                    }

            return _items;

        }

        private static IEnumerable<Type> GetTypes()
        {

            var ass = _controllerBaseType.Assembly;
            var i = AppDomain.CurrentDomain.GetAssemblies().Where(c => c.GetReferencedAssemblies().Any(d => d.Name == ass.GetName().Name)).ToList();
            foreach (var item in i)
            {

                var type2 = item.ExportedTypes.Where(c => _controllerBaseType.IsAssignableFrom(c.BaseType)).ToList();
                foreach (var itemType in type2)
                    if (itemType.GetCustomAttributes(typeof(ApiControllerAttribute), true).Any())
                        yield return itemType;

            }

        }

        private static readonly Type _controllerBaseType;
        private static readonly string _match;
    }

}
