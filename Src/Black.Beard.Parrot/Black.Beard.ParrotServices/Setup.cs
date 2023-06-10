using Bb.Json.Jslt.CustomServices;
using Bb.ParrotServices.Middlewares;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.OpenApi.Models;
using NLog.Web;

namespace Bb.ParrotServices
{


    public class Setup
    {

        public Setup()
        {
            this._datas = new Dictionary<string, object>();
        }

        public Setup Initialize(WebApplicationBuilder builder)
        {
            this._builder = builder;
            return this;
        }

        public Setup Initialize(string[] args)
        {
            _builder = WebApplication.CreateBuilder(args);
            return this;
        }        

        /// <summary>
        /// Add a variable
        /// </summary>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Setup Variable(string varName, object value)
        {
            this._datas.Add(varName, value);
            return this;
        }

        public Setup Services()
        {

            _builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            _builder.Services.AddEndpointsApiExplorer();                        
            return this;

        }

        public Setup Services(Action<WebApplicationBuilder> action)
        {

            // NLog: Setup NLog for Dependency injection
            _builder.Logging.ClearProviders();
            _builder.Host.UseNLog();

            action(_builder);
            return this;
        }

        public Setup Build()
        {
            _app = _builder.Build();
            return this;
        }

        public Setup Configure()
        {
            return Configure(_app);
        }

        public Setup Configure(WebApplication app)
        {

            this._app = app;            

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseMiddleware<ReverseProxyMiddleware>();

            return this;

        }

        public Setup Configure(Action<WebApplication> action)
        {
            action(_app);
            return this;
        }

        public Setup Run()
        {
            try
            {
                
                _app.Run();

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }

            return this;
        }

        private WebApplicationBuilder _builder;
        private WebApplication _app;
        private readonly Dictionary<string, object> _datas;
    }


}
