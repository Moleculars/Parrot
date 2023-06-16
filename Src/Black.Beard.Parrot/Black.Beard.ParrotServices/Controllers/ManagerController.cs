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
using System.Diagnostics;
using SharpYaml.Model;
using Microsoft.AspNetCore.Authorization;

namespace Bb.ParrotServices.Controllers
{


    [ApiController]
    [Route("[controller]/{template}")]
    [Authorize(Policy = "Manager")]
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
        /// <param name="upfile">The upfile that contains the contract in open api 3.*.</param>
        /// <returns>Return the list of template.</returns>
        /// <exception cref="Bb.ParrotServices.Exceptions.BadRequestException">No file received</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProjectDocument))]
        [HttpPost("{contract}/upload")]
        //[Consumes("multipart/form-data")]
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

            Trace.WriteLine($"{template} service {contract} has been generated", "info");
            return Ok(result);

        }

        /// <summary>
        /// Gets the generated services with the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProjectDocument>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("list")]
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
        [HttpPut("{contract}/build")]
        [Produces("application/json")]
        public async Task<IActionResult> Build([FromRoute] string template, [FromRoute] string contract)
        {

            var project = _builder.Contract(contract);
            var templateObject = project.Template(template);

            templateObject.Build();

            return Ok();

        }

        /// <summary>
        /// Runs the specified template & contract.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{contract}/run")]
        [Produces("application/json")]
        public async Task<IActionResult> Run([FromRoute] string template, [FromRoute] string contract)
        {

            var host = HttpContext.Request.Host.Host;

            var project = _builder.Contract(contract);
            var templateObject = project.Template(template);

            await templateObject.Build();

            var ports = await templateObject.Run(host, GetHttpPort(), GetHttpsPort()); // todo : comment retrouver le host name
            return Ok(ports);

        }


        /// <summary>
        /// kills the specified template & contract.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{contract}/kill")]
        [Produces("application/json")]
        public async Task<IActionResult> Kill([FromRoute] string template, [FromRoute] string contract)
        {

            var project = _builder.Contract(contract);
            var templateObject = project.Template(template);

            var result = await templateObject.Kill();

            return Ok(result);

        }


        /// <summary>
        /// return the services runnings. every service runs is tested
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProjectRunning>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("runnings")]       
        [Produces("application/json")]
        public async Task<IActionResult> Runnings([FromRoute] string template)
        {
            var items = await _builder.ListRunningsByTemplate(template);
            return Ok(items);
        }


        /// <summary>
        /// Download the specified data template.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <returns></returns>
        /// <exception cref="Bb.ParrotServices.Exceptions.BadRequestException">No file received</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{contract}/download_template")]
        //[Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> DownloadDataTemplate([FromRoute] string template, [FromRoute] string contract, [FromQuery] string filename)
        {

            var project = _builder.Contract(contract);
            ProjectBuilderTemplate templateObject = project.Template(template);

            var dir = templateObject.GetDirectoryProject("Templates");
            var files = templateObject.GetFiles(dir, filename);
            if (files.Length == 1)
            {
                return File(System.IO.File.OpenRead(files[0].FullName), "application/octet-stream", System.IO.Path.GetFileName(filename));
            }
            return NotFound();

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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{contract}/upload_template")]
        [Produces("application/json")]
        public async Task<IActionResult> UploadDataTemplate([FromRoute] string template, [FromRoute] string contract, IFormFile file)
        {

            var project = _builder.Contract(contract);
            ProjectBuilderTemplate templateObject = project.Template(template);

            var dir = templateObject.GetDirectoryProject("Templates");
            var files = templateObject.GetFiles(dir, file.FileName);

            if (files.Length == 1)
            {
                using (var stream = new FileStream(file.FileName, FileMode.CreateNew))
                {
                    await file.CopyToAsync(stream);
                }
                return Ok();
            }

            return NotFound();

        }


        private Uri? _http { get; }
        private Uri? _https { get; }

        private int? GetHttpPort() => _http != null ? _http.Port : null;
        private int? GetHttpsPort() => _https != null ? _https.Port : null;

        internal readonly ProjectBuilderProvider _builder;
        private readonly ILogger<ManagerController> _logger;

    }


}
