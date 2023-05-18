using System.Runtime.Serialization;

namespace Black.Beard.ParrotServices.Exceptions
{
    [Serializable]
    public class ParrotException : Exception
    {
        public ParrotException() : base() { }

        public ParrotException(string message) : base(message) { }

        public ParrotException(string message, Exception inner) : base(message, inner) { }

        protected ParrotException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


}