using Bb.Models.Security;
using System.Diagnostics.Contracts;
using System.Security.Claims;

namespace Bb.Extensions
{

    /// <summary>
    /// Api key extension
    /// </summary>
    public static class ApiKeyExtension
    {

        /// <summary>
        /// Consolidate api keys and translates in claim's list.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static List<Claim> Consolidate(this List<ApiKey> self)
        {

            HashSet<string> _h = new HashSet<string>();

            var claims = new List<Claim>();


            foreach (var apiKey in self)
            {

                claims.Add(new Claim(ClaimTypes.Name, apiKey.Owner));

                if (apiKey.Admin && _h.Add("Administrator"))
                    claims.Add(new Claim(ClaimTypes.Role, "Administrator"));

                foreach (var contract in apiKey.Contracts)
                {
                    var value = "Admin:" + contract;
                    if (_h.Add(value))
                        claims.Add(new Claim(ClaimTypes.Role, value));
                }

                foreach (var claim in apiKey.Claims)
                    if (_h.Add(claim.Key))
                        claims.Add(new Claim(claim.Key, claim.Value));

            }

            return claims;

        }

    }

}
