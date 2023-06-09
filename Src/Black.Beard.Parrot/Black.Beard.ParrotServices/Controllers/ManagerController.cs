using Bb.Process;
using Bb.OpenApiServices;
using Bb.ParrotServices.Exceptions;
using Bb.ParrotServices.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using Bb.Services;
using Bb.Models;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;

namespace Bb.ParrotServices.Controllers
{


    [ApiController]
    [Route("[controller]/{template}")]
    public class ManagerController : ControllerBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="server">The server.</param>
        public ManagerController(ILogger<ManagerController> logger, ProjectBuilderProvider builder, IServer server)
        {

            _builder = builder;
            _logger = logger;

            var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses?.ToArray();
            if (addresses != null)
            {
                foreach (string? address in addresses)
                {
                    var a = new Uri(address);
                    if (a.Scheme == "http")
                        this._http = a;
                    else
                        this._https = a;
                }
            }

        }

        /// <summary>
        /// Uploads the data source contract on server and generate the project for the specified contract.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <param name="upfile">The upfile.</param>
        /// <returns>Return the list of template.</returns>
        /// <exception cref="Bb.ParrotServices.Exceptions.BadRequestException">No file received</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProjectDocument))]
        [HttpPost("{contract}/upload")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [RequestSizeLimit(100_000_000)]
        //[DisableRequestSizeLimit]
        public async Task<IActionResult> UploadOpenApiContract([FromRoute] string template, [FromRoute] string contract, IFormFile upfile)
        {

            // verify fileInfo
            if (string.IsNullOrEmpty(upfile?.FileName))
                throw new BadRequestException("No file received");

            if (upfile == null || upfile.Length == 0)
                return Content("file stream is not selected or empty");


            var project = _builder.Contract(contract);
            var templateObject = project.Template(template);


            // Save contract
            var filepath = templateObject.GetPath("contract.json");
            var f = new FileInfo(filepath);
            if (f.Exists)
                f.Delete();
            templateObject.WriteOnDisk(upfile, filepath);

            // Generate project
            var result = templateObject.GenerateProject(filepath);

            return Ok(result);

        }

        /// <summary>
        /// Gets the generated services with the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProjectDocument>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> GetGeneratedServicesByTemplate([FromRoute] string template)
        {
            var items = _builder.ListByTemplate(template);
            return Ok(items);
        }

        /// <summary>
        /// Builds the specified contract.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{contract}/build")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Build([FromRoute] string template, [FromRoute] string contract)
        {

            var project = _builder.Contract(contract);
            var templateObject = project.Template(template);

            if (templateObject.Exists())
                templateObject.Build();

            else
            {

            }

            return Ok();

        }

        /// <summary>
        /// Runs the specified contract.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{contract}/run")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Run([FromRoute] string template, [FromRoute] string contract)
        {

            var host = HttpContext.Request.Host.Host;

            var project = _builder.Contract(contract);
            var templateObject = project.Template(template);

            if (templateObject.Exists())
            {
                var ports = templateObject.Run(host, GetHttpPort(), GetHttpsPort()); // todo : comment retrouver le host name
                return Ok(ports);
            }

            else
            {

            }

            return Ok();

        }


        /// <summary>
        /// return the services runnings. every service runs is tested
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProjectRunning>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("runnings")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Runnings([FromRoute] string template)
        {
            var items = await _builder.ListRunningsByTemplate(template);
            return Ok(items);
        }


        /// <summary>
        /// Uploads the data template.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        /// <exception cref="Bb.ParrotServices.Exceptions.BadRequestException">No file received</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{contract}/upload_template")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> UploadDataTemplate([FromRoute] string template, [FromRoute] string contract, IFormFile file)
        {

            // verify fileInfo
            if (string.IsNullOrEmpty(file?.FileName))
                throw new BadRequestException("No file received");

            if (file == null || file.Length == 0)
                return Content("file stream is not selected or empty");


            string path = Path.Combine(Directory.GetCurrentDirectory(), "services", contract);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            var filepath = Path.Combine(path, "template.json");
            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok();

        }

        

        private Uri? _http { get; }
        private Uri? _https { get; }


        private int? GetHttpPort() => _http != null ? _http.Port : null;
        private int? GetHttpsPort() => _https != null ? _https.Port : null;

        internal readonly ProjectBuilderProvider _builder;
        private readonly ILogger<ManagerController> _logger;

    }


}