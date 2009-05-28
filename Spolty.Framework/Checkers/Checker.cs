using System;

namespace Spolty.Framework.Checkers
{
    internal class Checker
    {
        internal static T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                ThrowArgumentNullException(parameterName);
            }
            return value;
        }

        internal static void ThrowArgumentNullException(string parameterName)
        {
            throw ArgumentNull(parameterName);
        }

        internal static ArgumentNullException ArgumentNull(string parameter)
        {
            return new ArgumentNullException(parameter);
        }
    }
}