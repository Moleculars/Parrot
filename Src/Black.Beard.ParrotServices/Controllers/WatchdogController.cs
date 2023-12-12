using Bb.Mock;
using Bb.Services.Managers;
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

            WatchdogResult result
                = new WatchdogResult(
                    new WatchdogResultItem("current_datetime", DateTime.UtcNow.ToString("u")),
                    new WatchdogResultItem("os_version", Environment.OSVersion.ToString())
                    );

            var list = await _builder.List();

            foreach (var item in list)
            {
                string value = "Stopped";
                Models.ProjectRunning r = await item.IsRunnings();
                if (r != null)
                    if (r.Started)
                        value = "Started";

                result.Items.Add(new WatchdogResultItem($"{item.Template}/{item.Contract}", value));

            }

            return this.Ok(result);

        }

        private ProjectBuilderProvider _builder;
        public ILogger<WatchdogController> _logger;

    }

}