using System;
using Bb.Json.Jslt.Services;
using System.Text;
using Oldtonsoft.Json;
using Oldtonsoft.Json.Linq;


namespace Bb.ParrotServices
{

    public class ServiceProcessor<TTarget>
    {

        public ServiceProcessor()
        {
            _serializer = new JsonSerializer();
            this._sources = new Dictionary<string, JToken>();
        }

        public TTarget? GetDatas(string templateFile, string datas)
        {

            TTarget? result = default;

            // Intialization of the configuration
            var configuration = new TranformJsonAstConfiguration()
            {
                OutputPath = Environment.CurrentDirectory,
            };


            TemplateTransformProvider Templateprovider = new TemplateTransformProvider(configuration);


            //Build the template translator
            var sbPayloadTemplate = new StringBuilder(templateFile.LoadFromFile());
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
            catch (Exception)
            {
                throw;
            }

            return result;

        }

        public void Add(string keyVariable, object value)
        {
            if (value is JToken t)
                _sources.Add(keyVariable, t);
            else
                _sources.Add(keyVariable, JToken.FromObject(value));
        }

        private readonly JsonSerializer _serializer;
        private IDictionary<string, JToken> _sources;


    }


}
