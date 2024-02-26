using Bb.Process;
using Bb.OpenApiServices;
using Bb.ParrotServices.Exceptions;
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
using Bb.Services.Managers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Bb.Analysis;
using Microsoft.Extensions.Logging;
using Bb.Analysis.Traces;

namespace Bb.ParrotServices.Controllers
{

    /// <summary>
    /// controller for manager the generator list
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "Generator")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(HttpExceptionModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(HttpExceptionModel))]
    public class GeneratorController : ControllerBase
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="server">The server.</param>
        public GeneratorController(ILogger<ManagerController> logger, ProjectBuilderProvider builder, IServer server)
        {
            _builder = builder;
            _logger = logger;                        
        }

        /// <summary>
        /// Uploads the generator.
        /// </summary>
        /// <param name="upfile">The upfile that contains the contract in open api 3.*.</param>
        /// <returns>Return the list of template.</returns>
        /// <exception cref="Bb.ParrotServices.Exceptions.BadRequestException">No file received.</exception>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectDocument))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("upload")]
        [Produces("application/json")]
        [RequestSizeLimit(100_000_000)]
        //[DisableRequestSizeLimit]
        public async Task<IActionResult> UploadGenerator(IFormFile upfile)
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


            _builder.AddGeneratorAssembly(upfile);


            //ProjectBuilderTemplate templateObject;
            //var project = _builder.Contract(contract);
            //try
            //{
            //    templateObject = project.Template(template);
            //}
            //catch (MockHttpException e)
            //{
            //    _logger.LogError(e, e.Message);
            //    return NotFound(e.Message);
            //}

            // Save contract
            //templateObject.WriteOnDisk(upfile);

            // Generate project


            var diag = new ScriptDiagnostics();
            diag.AddError(TextLocation.Empty, "Project generation failed", "Project generation failed");

            return Ok();

        }

        /// <summary>
        /// Gets the existing generated services list for the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>returns the list of existing generated list (launch or not) with jslt template list.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProjectDocuments))]
        [HttpGet("list")]
        [Produces("application/json")]
        public async Task<IActionResult> GetGeneratedServicesByTemplate()
        {            
            return Ok();
        }             

        private readonly ProjectBuilderProvider _builder;
        private readonly ILogger<ManagerController> _logger;


    }

}


// dotnet.exe build "/app/tmp/parrot/projects/parcel/mock/service/mock.csproj