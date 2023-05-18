using Black.Beard.OpenApiServices;
using Black.Beard.ParrotServices.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;

namespace Black.Beard.ParrotServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MockController : ControllerBase
    {


        private readonly ILogger<MockController> _logger;

        public MockController(ILogger<MockController> logger)
        {
            _logger = logger;
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("upload_openapi/{contract}")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> UploadOpenApiContract([FromRoute] string contract, [FromForm] IFormFile file)
        {

            // verify fileInfo
            if (string.IsNullOrEmpty(file?.FileName))
                throw new BadRequestException("No file received");
            
            if (file == null || file.Length == 0)
                return Content("file stream is not selected or empty");


            string path = Path.Combine(Directory.GetCurrentDirectory(), "services", contract);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            var filepath = Path.Combine(path, "contract.json");
            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            
            //var name = "Black.Beard.Mock";
            //var generator = new MockServiceGenerator(name, path)
            //{
            //    Namespace = name
            //}
            //.InitializeContract(filepath)
            //.ConfigureProject(prj =>
            //{

            //})
            //.Generate()
            //;

            return Ok();

        }



        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("upload_data_template/{contract}")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> UploadDataTemplate([FromRoute] string contract, [FromForm] IFormFile file)
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


            //var name = "Black.Beard.Mock";
            //var generator = new MockServiceGenerator(name, path)
            //{
            //    Namespace = name
            //}
            //.InitializeContract(filepath)
            //.ConfigureProject(prj =>
            //{
            //})
            //.Generate()
            //;

            return Ok();

        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("upload_data/{contract}")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> UploadData([FromRoute] string contract, [FromForm] IFormFile file)
        {

            // verify fileInfo
            if (string.IsNullOrEmpty(file?.FileName))
                throw new BadRequestException("No file received");

            if (file == null || file.Length == 0)
                return Content("file stream is not selected or empty");


            string path = Path.Combine(Directory.GetCurrentDirectory(), "services", contract);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            var filepath = Path.Combine(path, file.FileName);
            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }


            //var name = "Black.Beard.Mock";
            //var generator = new MockServiceGenerator(name, path)
            //{
            //    Namespace = name
            //}
            //.InitializeContract(filepath)
            //.ConfigureProject(prj =>
            //{
            //})
            //.Generate()
            //;

            return Ok();

        }
    
    }


}