namespace Bb.Models
{
    public class HttpExceptionModel
    {

        public string TraceIdentifier { get; set; }

        public string Message { get; set; } = "Sorry, an error has occurred. Please contact our customer service with TraceIdentifier for assistance.";
        
        public ISession Session { get; set; }

    }
}
