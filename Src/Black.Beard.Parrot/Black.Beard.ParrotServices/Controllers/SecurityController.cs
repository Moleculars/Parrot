using Bb.Mock;
using Bb.Models.Security;
using Bb.Services;
using Bb.Services.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharpYaml.Tokens;
using System.ComponentModel;

#pragma warning disable CS8618, CS1591
namespace Bb.ParrotServices.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "Security")]
    public class SecurityController : Controller
    {

        public SecurityController(ProjectBuilderProvider builder, ILogger<WatchdogController> logger, IApiKeyRepository apikeys)
        {
            _logger = logger;
            _apikeys = apikeys;
        }

        /// <summary>
        /// return the list of api key
        /// </summary>
        /// <param name="page">number of the page</param>
        /// <param name="count">count of item by page</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiItem[]))]
        [Produces("application/json")]
        [HttpGet("keys/{page}/{count}")]
        public async Task<IActionResult> GetList([FromRoute] int page, [FromRoute] int count)
        {
            var results = _apikeys.GetList(page, count);
            var pages = _apikeys.GetPageCount(count);
            HttpContext.Response.Headers.Add("X-Pagination", pages.ToString());
            await Task.Yield();
            return this.Ok(results);
        }

        /// <summary>
        /// return the api key information specified by id
        /// </summary>
        /// <param name="id">id of the the api key</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiKey))]
        [Produces("application/json")]
        [HttpGet("getkeybyid/{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
                var result = _apikeys.GetById(id);
                await Task.Yield();
                return this.Ok(result);
        }

        /// <summary>
        /// return the api key information specified by id
        /// </summary>
        /// <param name="id">id of the the api key</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiKey))]
        [Produces("application/json")]
        [HttpGet("getkey/{key}")]
        public async Task<IActionResult> GetByKey([FromRoute] string key)
        {
                var result = _apikeys.GetByKey(key);
                await Task.Yield();
                return this.Ok(result);
        }

        /// <summary>
        /// Add an api key in the referential. the first key is allways administror
        /// </summary>
        /// <param name="data"> <see cref="T:ApiKey"/></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult[]))]
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPost("upsertkeys")]
        public async Task<IActionResult> Set([FromBody] ApiKeyModel[] datas)
        {
                var results = _apikeys.Set(datas);
                await Task.Yield();
                return this.Ok(results);
        }

        /// <summary>
        /// remove item specified by id
        /// </summary>
        /// <param name="id"> <see cref="T:ApiKey"/></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult))]
        [Produces("application/json")]
        [HttpPost("deletekeybyid/{ids}")]
        public async Task<IActionResult> DelById([FromBody] Guid id)
        {
                var results = _apikeys.DelById(new Guid[] { id });
                await Task.Yield();
                return this.Ok(results);
        }

        /// <summary>
        /// remove item specified by key
        /// </summary>
        /// <param name="key"> <see cref="T:ApiKey"/></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult))]
        [Produces("application/json")]
        [HttpPost("deletekey/{key}")]
        public async Task<IActionResult> DelByKey([FromBody] string key)
        {
                var results = _apikeys.DelByKey(new string[] { key });
                await Task.Yield();
                return this.Ok(results);
        }

        public ILogger<WatchdogController> _logger;
        private readonly IApiKeyRepository _apikeys;

    }

}