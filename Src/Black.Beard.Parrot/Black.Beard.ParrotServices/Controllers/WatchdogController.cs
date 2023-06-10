﻿using Bb.Mock;
using Bb.ParrotServices.Services;
using Flurl.Util;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS8618, CS1591
namespace Bb.ParrotServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WatchdogController : Controller
    {

        public WatchdogController(ProjectBuilderProvider builder, ILogger<WatchdogController> logger)
        {
            _builder = builder;
            _logger = logger;
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WatchdogResult))]
        [HttpGet("isupandrunning")]
        [Produces("application/json")]
        public async Task<IActionResult> IsUpAndRunning()
        {
            try
            {
                
                WatchdogResult result 
                    = new WatchdogResult(new WatchdogResultItem("current_datetime", DateTime.UtcNow.ToString("u")));

                var list = await _builder.List();

                foreach (var item in list)
                {
                    string value = "Stopped";
                    Models.ProjectRunning r = await item.ListRunnings();
                    if (r != null)
                        if (r.Started)
                            value = "Started";

                    result.Items.Add(new WatchdogResultItem($"{item.Template}/{item.Contract}", value));
                    
                }


                return this.Ok(result);

            }
            catch (Exception ex)
            {
                Guid errorId = Guid.NewGuid();
                _logger.LogError(ex, ex.Message, errorId);
                return this.BadRequest(new WatchdogResultException(errorId, "Sorry, an error has occurred. Please contact our customer service with uuid for assistance."));
            }
        }

        private ProjectBuilderProvider _builder;
        public ILogger<WatchdogController> _logger;

    }
}