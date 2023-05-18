using System;
using Bb.Json.Jslt.Services;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using Bb;


namespace Black.Beard.OpenApiServices.Embedded
{

    public class ServiceProcessor<TTarget>
    {

        public ServiceProcessor()
        {
            _serializer = new JsonSerializer();
            this._sources = new Dictionary<string, object>();
        }

        public TTarget GetDatas(string templateFile, string datas)
        {

            TTarget result = default(TTarget);

            // Intialization of the configuration
            var configuration = new TranformJsonAstConfiguration()
            {
                OutputPath = Environment.CurrentDirectory,
            };


            TemplateTransformProvider Templateprovider = new TemplateTransformProvider(configuration);


            //Build the template translator
            var sbPayloadTemplate = new StringBuilder(templateFile.LoadFile());
            JsltTemplate template = Templateprovider.GetTemplate(sbPayloadTemplate, false, "name of the template file");


            // load mocked datas from file source
            // Create the sources object with the primary source of data and datas argument of the service
            var source = SourceJson.GetFromFile(datas);
            var src = new Sources(source);
            src.Variables.Add(this._sources);



            try
            {
                RuntimeContext ctx = template.Transform(src);
                var payload = ctx.TokenResult;
                result = payload.ToObject<TTarget>(_serializer);
            }
            catch (Exception ex)
            {

                throw;
            }

            return result;

        }

        public void Add(string keyVariable, object value)
        {
            _sources.Add(keyVariable, value);
        }

        private readonly JsonSerializer _serializer;
        private IDictionary<string, object> _sources;


    }


}
