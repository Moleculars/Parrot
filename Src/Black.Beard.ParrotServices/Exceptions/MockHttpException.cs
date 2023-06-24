using System.Runtime.Serialization;

namespace Bb.ParrotServices.Exceptions
{
    [Serializable]
    public class MockHttpException : ParrotException
    {
        virtual public int HttpResponseCode { get; } = 500;

        virtual public string Value { get; } = "Internal Server Error";

        public MockHttpException() : base() { }

        public MockHttpException(string message) : base(message) { }

        public MockHttpException(string message, Exception inner) : base(message, inner) { }

        protected MockHttpException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


}