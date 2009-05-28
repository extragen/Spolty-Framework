using System;
using System.Runtime.Serialization;

namespace Spolty.Framework.Exceptions
{
    [Serializable]
    public class SpoltyException : ApplicationException
    {
        public SpoltyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SpoltyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SpoltyException(string message) : base(message)
        {
        }

        public SpoltyException()
        {
        }
    }
}