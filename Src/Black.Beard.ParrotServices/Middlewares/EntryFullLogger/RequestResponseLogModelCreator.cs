using Bb.ComponentModel.Attributes;
using Newtonsoft.Json;

namespace Bb.Middlewares.EntryFullLogger
{
    [ExposeClass(Context = Constants.Models.Service, ExposedType = typeof(IRequestResponseLogModelCreator), LifeCycle = IocScopeEnum.Singleton)]
    public class RequestResponseLogModelCreator : IRequestResponseLogModelCreator
    {
        public RequestResponseLogModel LogModel { get; private set; }

        public RequestResponseLogModelCreator()
        {
            LogModel = new RequestResponseLogModel();
        }

        public string LogString()
        {
            var jsonString = JsonConvert.SerializeObject(LogModel, Formatting.Indented);
            return jsonString;
        }
    }


}
