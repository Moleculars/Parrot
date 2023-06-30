using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Bb.Mocks
{

    public class HttpExceptionModel
    {

        public HttpExceptionModel()
        {
        }

        public string TraceIdentifier { get; set; }

        public string Origin { get; set; }

        public string Message { get; set; } = "Sorry, an error has occurred. Please contact our customer service with TraceIdentifier for assistance.";
        
        public ISession Session { get; set; }

    }
}
