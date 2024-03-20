using Bb.ParrotServices.Exceptions;
using Bb.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Authorization;
using Bb.Services.Managers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Bb.Extensions;
using Bb.Analysis.DiagTraces;
using Bb.Compilers;

namespace Bb.ParrotServices.Controllers
{

    [ApiController]
    [Route("[controller]/{template}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(HttpExceptionModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(HttpExceptionModel))]
    public class GeneratorController : ControllerBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="server">The server.</param>
        public GeneratorController(ILogger<GeneratorController> logger, ProjectBuilderProvider builder, IServer server)
        {

            _builder = builder;
            _logger = logger;

            var addresses = server.GetServerAcceptedAddresses();
            foreach (Uri address in addresses)
            {
                if (address.Scheme.ToLower() == "http")
                    this._http = address;
                else
                    this._https = address;
            }

        }

        /// <summary>
        /// Uploads the data source contract on server and generate the project for the specified contract.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <param name="upfile">The file to upload that contains the contract in open api 3.*.</param>
        /// <returns>Return the list of template.</returns>
        /// <exception cref="Bb.ParrotServices.Exceptions.BadRequestException">No file received.</exception>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectDocument))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{contract}/upload")]
        //[Consumes("form-data")]
        [Produces("application/json")]
        [RequestSizeLimit(100_000_000)]
        //[DisableRequestSizeLimit]
        public async Task<IActionResult> UploadOpenApiContract([FromRoute] string template, [FromRoute] string contract, IFormFile upfile)
        {

            _logger.LogDebug("Upload root : {root}", _builder.Root);

            // verify fileInfo
            if (string.IsNullOrEmpty(upfile?.FileName))
            {
                _logger.LogError("No file was received");
                throw new BadRequestException("No file was received");
            }

            if (upfile == null || upfile.Length == 0)
            {
                _logger.LogError("file stream is not selected or empty");
                return BadRequest("file stream is not selected or empty");
            }

            ProjectBuilderTemplate templateObject;
            var project = _builder.Contract(contract, true);
            try
            {
                templateObject = project.Template(template, true);
            }
            catch (MockHttpException e)
            {
                _logger.LogError(e, e.Message);
                return NotFound(e.Message);
            }

            // Save contract
            templateObject.WriteOnDisk(upfile);

            // Generate project
            var result = templateObject.GenerateProject();

            if (result != null && result.Context != null)
            {
                if (result.Context.Diagnostics.Success)
                {
                    _logger.LogInformation($"{template}/{contract} has been generated");
                    return Ok(result);
                }
                else
                    _logger.LogError($"Failed to generate {template}/{contract}");

                return BadRequest(GetBadModel(result.Context.Diagnostics));
            }

            var diag = new ScriptDiagnostics();
            diag.AddError(TextLocation.Empty, "Project generation failed", "Project generation failed");

            return BadRequest(GetBadModel(diag));

        }

        /// <summary>
        /// Gets the existing generated services list for the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>returns the list of existing generated list (launch or not) with jslt template list.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectDocuments))]
        [HttpGet("list")]
        [Produces("application/json")]
        public async Task<IActionResult> GetGeneratedServicesByTemplate([FromRoute] string template)
        {
            var items = _builder.ListByTemplate(template);
            return Ok(items);
        }

        /// <summary>Builds the specified template and contract.</summary>
        /// <param name="template">Generation template name. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <exception cref="T:NotFoundObjectResult">the template is not found</exception>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpPost("{contract}/build")]
        [Produces("text/plain")]
        public async Task<IActionResult> Build([FromRoute] string template, [FromRoute] string contract)
        {

            _logger.LogDebug("build root : {root}", _builder.Root);

            var project = _builder.Contract(contract);

            if (project == null)
                return NotFound(contract + " not found");

            ProjectBuilderTemplate templateObject;
            try
            {
                templateObject = project.Template(template);
            }
            catch (MockHttpException e)
            {
                _logger.LogError(e, e.Message);
                return NotFound(e.Message);
            }

            Compilers.AssemblyResult? result = default;
            try
            {

                _logger.LogDebug("build target template : {root}", templateObject.Root);
                result = await templateObject.Build();

                if (!result.Success)
                {

                    foreach (ScriptDiagnostic item in result.Diagnostics.Errors)
                        _logger.LogError(item.ToString());

                    return BadRequest(result.Diagnostics.Errors);

                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();

        }

        /// <summary>
        /// Runs the specified template and contract.
        /// </summary>
        /// <param name="template">Generation template name. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <exception cref="T:NotFoundObjectResult">the template is not found</exception>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{contract}/run")]
        [Produces("application/json")]
        public async Task<IActionResult> Run([FromRoute] string template, [FromRoute] string contract)
        {

            var host = HttpContext.Request.Host.Host;

            var project = _builder.Contract(contract);

            ProjectBuilderTemplate templateObject;
            try
            {
                templateObject = project.Template(template);
            }
            catch (MockHttpException e)
            {
                _logger.LogError(e, e.Message);
                return NotFound(e.Message);
            }


            if (templateObject.IsRunnings())
                return BadRequest("the service is already running");


            Compilers.AssemblyResult result = await templateObject.Build();

            if (!result.Success)
            {

                foreach (ScriptDiagnostic item in result.Diagnostics.Errors)
                    _logger.LogError(item.ToString());

                return BadRequest(result.Diagnostics.Errors);

            }


            ServiceHost? ports = null;
            try
            {
                ports = await templateObject.Run(result, host, GetHttpPort(), GetHttpsPort()); // todo : comment retrouver le hostname
            }
            catch (Exception ex)
            {
                throw;
            }


            if (ports == null)
                return BadRequest("failed to run the path {template}/{contract}");

            return Ok(ports);


        }


        /// <summary>
        /// kills the specified template & contract.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <returns></returns>
        /// <exception cref="T:NotFoundObjectResult">the template is not found</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{contract}/kill")]
        [Produces("application/json")]
        public async Task<IActionResult> Kill([FromRoute] string template, [FromRoute] string contract)
        {

            var project = _builder.Contract(contract);

            ProjectBuilderTemplate templateObject;
            try
            {
                templateObject = project.Template(template);
            }
            catch (MockHttpException e)
            {
                _logger.LogError(e, e.Message);
                return NotFound(e.Message);
            }

            var result = await templateObject.Kill();

            return Ok(result);

        }


        /// <summary>
        /// return the services runnings. every service runs is tested
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProjectInfo>))]
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
        /// <param name="filename">the filename that you want download..</param>
        /// <returns></returns>
        /// <exception cref="T:NotFoundObjectResult">the template is not found</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{contract}/download_template")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> DownloadDataTemplate([FromRoute] string template, [FromRoute] string contract, [FromQuery] string filename)
        {

            var project = _builder.Contract(contract);

            ProjectBuilderTemplate templateObject;
            try
            {
                templateObject = project.Template(template);
            }
            catch (MockHttpException e)
            {
                _logger.LogError(e, e.Message);
                return NotFound(e.Message);
            }

            var dir = templateObject.GetDirectoryProject("Templates");
            var files = templateObject.GetFiles(dir, filename);
            if (files.Length == 1)
            {
                return File(System.IO.File.OpenRead(files[0].FullName), "application/octet-stream", System.IO.Path.GetFileName(filename));
            }
            return NotFound();

        }


        /// <summary>
        /// Uploads the data template and replace the existing file. The template is not tested.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <param name="upfile">The file to replace.</param>
        /// <returns></returns>
        /// <exception cref="T:BadRequestObjectResult">No file received</exception>
        /// <exception cref="T:NotFoundObjectResult">the template is not found</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{contract}/upload_template")]
        public async Task<IActionResult> UploadDataTemplate([FromRoute] string template, [FromRoute] string contract, IFormFile upfile)
        {

            // verify fileInfo
            if (string.IsNullOrEmpty(upfile?.FileName))
            {
                _logger.LogError("No file was received");
                throw new BadRequestException("No file was received");
            }

            if (upfile == null || upfile.Length == 0)
            {
                _logger.LogError("file stream is not selected or empty");
                return BadRequest("file stream is not selected or empty");
            }

            var project = _builder.Contract(contract);

            ProjectBuilderTemplate templateObject;
            try
            {
                templateObject = project.Template(template);
            }
            catch (MockHttpException e)
            {
                _logger.LogError(e, e.Message);
                return NotFound(e.Message);
            }

            var dir = templateObject.GetDirectoryProject("Templates");
            var files = templateObject.GetFiles(dir, upfile.FileName);

            if (files.Length == 1)
            {
                using (var stream = new FileStream(upfile.FileName, FileMode.CreateNew))
                {
                    await upfile.CopyToAsync(stream);
                }
                return Ok();
            }

            return NotFound(upfile.FileName);

        }


        internal readonly ProjectBuilderProvider _builder;

        private ModelStateDictionary GetBadModel(ScriptDiagnostics diagnostics)
        {

            var model = new ModelStateDictionary();

            foreach (var diagnostic in diagnostics)
            {

                string message;

                //if (diagnostic.Location.Start.IsEmpty)
                //{
                //}
                //else
                //    //message += diagnostic.Location.Start.;

                model.AddModelError(diagnostic.Severity + (model.Count + 1).ToString(), diagnostic.ToString());

            }

            return model;

        }

        private Uri? _http { get; }
        private Uri? _https { get; }
        private int? GetHttpPort() => _http != null ? _http.Port : null;
        private int? GetHttpsPort() => _https != null ? _https.Port : null;
        private readonly ILogger<GeneratorController> _logger;

    }


}


// dotnet.exe build "/app/tmp/parrot/projects/parcel/mock/service/mock.csproj