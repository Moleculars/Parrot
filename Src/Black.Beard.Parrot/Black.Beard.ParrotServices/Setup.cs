using log4net;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Black.Beard.ParrotServices
{


    public class Setup
    {

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

            _builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            _builder.Services.AddEndpointsApiExplorer();
            _builder.Services.AddSwaggerGen();

            return this;

        }

        public Setup Services(Action<WebApplicationBuilder> action)
        {
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
                LogManager.Flush(5000);
                LogManager.Shutdown();
            }

            return this;
        }

        private WebApplicationBuilder _builder;
        private WebApplication _app;

    }


}
