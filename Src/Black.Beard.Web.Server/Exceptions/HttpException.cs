using System.Runtime.Serialization;

namespace Bb.ParrotServices.Exceptions
{

    [Serializable]
    public class HttpException : Exception
    {
        public virtual int HttpResponseCode { get; } = 500;

        public virtual string Value { get; } = "Internal Server Error";

        public HttpException() : base() { }

        public HttpException(string message) : base(message) { }

        public HttpException(string message, Exception inner) : base(message, inner) { }

        protected HttpException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


}