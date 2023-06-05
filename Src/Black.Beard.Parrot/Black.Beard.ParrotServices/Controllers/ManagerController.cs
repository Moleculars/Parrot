using Bb.Process;
using Bb.OpenApiServices;
using Bb.ParrotServices.Exceptions;
using Bb.ParrotServices.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;

namespace Bb.ParrotServices.Controllers
{


    [ApiController]
    [Route("[controller]/{contract}/{template}")]
    public class ManagerController : ControllerBase
    {

        public ManagerController(ILogger<ManagerController> logger, ProjectBuilderProvider builder)
        {
            _builder = builder;
            _logger = logger;
        }

        /// <summary>
        /// Uploads the data source contract on server and generate the project for the specified contract.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="upfile">The upfile.</param>
        /// <returns></returns>
        /// <exception cref="Bb.ParrotServices.Exceptions.BadRequestException">No file received</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("upload_openapi")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [RequestSizeLimit(100_000_000)]
        //[DisableRequestSizeLimit]
        public async Task<IActionResult> UploadOpenApiContract([FromRoute] string contract, [FromRoute] string template, IFormFile upfile)
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
            templateObject.GenerateProject(filepath);

            return Ok();

        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("build")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Build([FromRoute] string contract, [FromRoute] string template)
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


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("run")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Run([FromRoute] string contract, [FromRoute] string template)
        {

            var request = HttpContext.Request;
            var port = request.Host.Port.HasValue ? request.Host.Port.Value : 5000;

            var project = _builder.Contract(contract);
            var templateObject = project.Template(template);

            if (templateObject.Exists())
            {
                var uris = templateObject.Run(port);
                return Ok(uris);
            }

            else
            {

            }

            return Ok();

        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("upload_template")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> UploadDataTemplate([FromRoute] string contract, IFormFile file)
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

        internal readonly ProjectBuilderProvider _builder;
        private readonly ILogger<ManagerController> _logger;


    }


}