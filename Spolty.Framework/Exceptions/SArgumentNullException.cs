using System;
using System.Runtime.Serialization;

namespace Spolty.Framework.Exceptions
{
    internal class SArgumentNullException : ArgumentNullException
    {
        public SArgumentNullException(string paramName) : base(paramName)
        {
        }

        public SArgumentNullException()
        {
        }

        public SArgumentNullException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SArgumentNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SArgumentNullException(string paramName, string message) : base(paramName, message)
        {
        }
    }
}