namespace Bb.OpenApiServices
{
    public class ServiceGeneratorProcess<T>
    {


        public ServiceGeneratorProcess(ContextGenerator ctx)
        {
            this._ctx = ctx;
            _services = new List<IServiceGenerator<T>>();
        }


        public ServiceGeneratorProcess<T> Append(params IServiceGenerator<T>[] services)
        {

            this._services.AddRange(services);
            return this;
        }

        internal ServiceGeneratorProcess<T> Generate(T document)
        {

            foreach (var service in _services)
            {
                try
                {

                    service.Parse(document, _ctx);

                }
                catch (Exception ex)
                {

                    throw;
                }

                if (!this._ctx.Diagnostics.Success)
                    return this;
            
            }

            return this;

        }

        private readonly ContextGenerator _ctx;
        private List<IServiceGenerator<T>> _services;

    }




}