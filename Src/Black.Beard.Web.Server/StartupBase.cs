using Bb.ComponentModel;
using Bb.Extensions;
using Bb.Models.Security;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Bb
{
    
    public class StartupBase
    {


        /// <summary>
        /// Auto discover all types with attribute [ExposeClass] for register  in ioc.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public virtual void DiscoversTypes(IServiceCollection services, IConfiguration configuration)
        {

            // Auto discover all types with attribute [ExposeClass] for register  in ioc.
            services.UseTypeExposedByAttribute(configuration, ConstantsCore.Configuration, c =>
            {
                services.BindConfiguration(c, configuration);
                //var cc1 = JsonSchema.FromType(c).ToJson();
                //var cc2 = c.GenerateContracts();
            })
            .UseTypeExposedByAttribute(configuration, Constants.Models.Model)
            .UseTypeExposedByAttribute(configuration, Constants.Models.Service);

        }


        /// <summary>
        /// evaluate permissions.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="policy">The policy.</param>
        /// <returns></returns>
        protected async Task<bool> Authorize(AuthorizationHandlerContext arg, PolicyModel policy)
        {

            if (arg.User != null)
            {

                var res = (DefaultHttpContext)arg.Resource;
                var path = res.Request.Path;

                PolicyModelRoute route = policy.Evaluate(path);

                var i = arg.User.Identity as ClaimsIdentity;
                var roles = i.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

                //if (roles.Where(c => c.Value == ""))

            }

            await Task.Yield();

            return true;

        }



    }

}
