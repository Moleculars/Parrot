
using Bb.Middlewares;
using Bb.Models.Security;
using Bb.ParrotServices.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using NLog.Web;

namespace Bb.ParrotServices
{

    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {

        public TokenAuthenticationOptions()
        {
            
        }

    }

    public static class SchemesNamesConst
    {
        public const string TokenAuthenticationDefaultScheme = "TokenAuthenticationScheme";
    }


    public class MinimumAgeRequirement : IAuthorizationRequirement
    {

        public MinimumAgeRequirement()
        {

        }

    }


  

}
