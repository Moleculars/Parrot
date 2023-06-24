using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

#pragma warning disable CS1591

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

        public Setup Services()
        {
            if (_builder != null)
            {
                _builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                _builder.Services.AddEndpointsApiExplorer();
            }
            return this;
        }

        public Setup Services(Action<WebApplicationBuilder> action)
        {
            if (_builder != null)
                action(_builder);
            return this;
        }

        public Setup Build()
        {
            if (_builder != null)
                _app = _builder.Build();
            return this;
        }

        public Setup Configure()
        {
            if (_app != null)
                return Configure(_app);
            return this;
        }

        public Setup Configure(WebApplication app)
        {

            this._app = app;

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();


            return this;

        }

        public Setup Configure(Action<WebApplication> action)
        {
            if (_app != null)
                action(_app);
            return this;
        }

        public Setup Run()
        {
            try
            {

                if (_app != null)
                    _app.Run();

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                LogManager.Flush(5000);
                LogManager.Shutdown();
            }

            return this;
        }

        private WebApplicationBuilder? _builder;
        private WebApplication? _app;
        private readonly Dictionary<string, object> _datas;

    }


}
