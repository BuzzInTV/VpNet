using System;
using VpNet.Exceptions;

namespace VpNet.Internal
{
    internal static class ThrowHelper
    {
        public static ArgumentNullException ArgumentNullException(string paramName) => new(paramName);

        public static void ThrowArgumentNullException(string paramName) => throw ArgumentNullException(paramName);

        public static InvalidOperationException CannotUseSelfException() => new(ExceptionMessages.CannotUseSelf);

        public static void ThrowCannotUseSelfException() => throw CannotUseSelfException();

        public static InvalidOperationException NotInWorldException() => new(ExceptionMessages.NotInWorld);

        public static void ThrowNotInWorldException() => throw NotInWorldException();

        public static ObjectNotFoundException ObjectNotFoundException() => new(ExceptionMessages.ObjectNotFound);

        public static void ThrowObjectNotFoundException() => throw ObjectNotFoundException();

        public static ArgumentException StringTooLongException(string paramName) =>
            new(ExceptionMessages.StringTooLong, paramName);

        public static void ThrowStringTooLongException(string paramName) => throw StringTooLongException(paramName);

        public static ArgumentOutOfRangeException ZeroThroughOneException(string paramName) =>
            new(paramName, ExceptionMessages.ZeroThroughOne);

        public static void ThrowZeroThroughOneException(string paramName) => throw ZeroThroughOneException(paramName);
    }
}
