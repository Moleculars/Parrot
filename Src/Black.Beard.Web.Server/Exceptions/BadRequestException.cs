using System.Runtime.Serialization;

namespace Bb.ParrotServices.Exceptions
{

    [Serializable]
    public class BadRequestException : HttpException
    {
        override public int HttpResponseCode { get; } = 400;

        override public string Value { get; } = "Bad Request";

        public BadRequestException() : base() { }

        public BadRequestException(string message) : base(message) { }

        public BadRequestException(string message, Exception inner) : base(message, inner) { }

        protected BadRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


}