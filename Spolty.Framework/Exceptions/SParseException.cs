using System;

namespace Spolty.Framework.Exceptions
{
    public sealed class SParseException : SpoltyException
    {
        private readonly int position;

        public SParseException(string message, int position)
            : base(message)
        {
            this.position = position;
        }

        public int Position
        {
            get { return position; }
        }

        public override string ToString()
        {
            return string.Format(Res.ParseExceptionFormat, Message, position);
        }
    }
}