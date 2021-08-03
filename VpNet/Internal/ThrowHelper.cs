using System;
using System.Threading.Tasks;
using VpNet.Exceptions;

namespace VpNet.Internal
{
    internal static class ThrowHelper
    {
        public static ArgumentNullException ArgumentNullException(string paramName) =>
            new(paramName);

        public static Task<ArgumentNullException> ArgumentNullExceptionAsync(string paramName) =>
            Task.FromException<ArgumentNullException>(ArgumentNullException(paramName));

        public static void ThrowArgumentNullException(string paramName) =>
            throw ArgumentNullException(paramName);

        public static InvalidOperationException CannotUseSelfException() =>
            new(ExceptionMessages.CannotUseSelf);

        public static Task<InvalidOperationException> CannotUseSelfExceptionAsync() =>
            Task.FromException<InvalidOperationException>(CannotUseSelfException());

        public static void ThrowCannotUseSelfException() =>
            throw CannotUseSelfException();

        public static InvalidOperationException NotInWorldException() =>
            new(ExceptionMessages.NotInWorld);

        public static Task<InvalidOperationException> NotInWorldExceptionAsync() =>
            Task.FromException<InvalidOperationException>(NotInWorldException());

        public static void ThrowNotInWorldException() =>
            throw NotInWorldException();

        public static ObjectNotFoundException ObjectNotFoundException() =>
            new(ExceptionMessages.ObjectNotFound);

        public static Task<ObjectNotFoundException> ObjectNotFoundExceptionAsync() =>
            Task.FromException<ObjectNotFoundException>(ObjectNotFoundException());

        public static void ThrowObjectNotFoundException() =>
            throw ObjectNotFoundException();

        public static ArgumentException StringTooLongException(string paramName) =>
            new(ExceptionMessages.StringTooLong, paramName);

        public static Task<ArgumentException> StringTooLongExceptionAsync(string paramName) =>
            Task.FromException<ArgumentException>(StringTooLongException(paramName));

        public static void ThrowStringTooLongException(string paramName) =>
            throw StringTooLongException(paramName);
    }
}
