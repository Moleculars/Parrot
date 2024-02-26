using System.Runtime.Serialization;

namespace Bb.ParrotServices.Exceptions
{

    [Serializable]
    public class MockHttpException : HttpException
    {

        public MockHttpException() : base() { }

        public MockHttpException(string message) : base(message) { }

        public MockHttpException(string message, Exception inner) : base(message, inner) { }

        protected MockHttpException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


}