using Bb.Mock;
using Bb.Models;
using Bb.Models.Security;
using Bb.ParrotServices.Services;
using Bb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;

#pragma warning disable CS8618, CS1591
namespace Bb.ParrotServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = SchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public class SecurityController : Controller
    {

        public SecurityController(ProjectBuilderProvider builder, ILogger<WatchdogController> logger, IApiKeyRepository apikeys)
        {
            _builder = builder;
            _logger = logger;
            _apikeys = apikeys;
        }

        /// <summary>
        /// Add an api key in the referential. the first key is altime the administror
        /// </summary>
        /// <param name="data"> <see cref="T:ApiKey"/></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult[]))]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPost("set")]
        public async Task<IActionResult> Set([FromBody] ApiKeyModel[] datas)
        {

            try
            {
                var results = _apikeys.Set(datas);
                return this.Ok(results);
            }
            catch (Exception ex)
            {
                Guid errorId = Guid.NewGuid();
                _logger.LogError(ex, ex.Message, errorId);
                return this.BadRequest(new WatchdogResultException(errorId, "Sorry, an error has occurred. Please contact our customer service with uuid for assistance."));
            }
        }

        /// <summary>
        /// Remove an api key in the referential.
        /// </summary>
        /// <param name="data"> <see cref="T:ApiKey"/></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult[]))]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPost("det")]
        public async Task<IActionResult> Del([FromBody] ApiKeyModel[] datas)
        {

            try
            {
                var results = _apikeys.Del(datas);
                return this.Ok(results);
            }
            catch (Exception ex)
            {
                Guid errorId = Guid.NewGuid();
                _logger.LogError(ex, ex.Message, errorId);
                return this.BadRequest(new WatchdogResultException(errorId, "Sorry, an error has occurred. Please contact our customer service with uuid for assistance."));
            }
        }

        public ILogger<WatchdogController> _logger;
        private readonly IApiKeyRepository _apikeys;

    }

}