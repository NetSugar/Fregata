using Fregata.Exceptions;
using System;
using System.Runtime.CompilerServices;

namespace Fregata.Utils
{
    internal static class ThrowHelper
    {
        internal static void ThrowDataLessThanReadException() => throw CreateDataLessThanReadException();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateDataLessThanReadException() => new DataLessThanReadException();

        internal static void ThrowLengthFieldConfigErrorException() => throw CreateLengthFieldConfigErrorException();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateLengthFieldConfigErrorException() => new LengthFieldConfigErrorException();

        internal static void ThrowServerNameNotBeNullErrorException() => throw CreateServerNameNotBeNullErrorException();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateServerNameNotBeNullErrorException() => new ServerNameNotBeNullException();

        internal static void ThrowListenEndPointNotBeNullErrorException() => throw CreateListenEndPointNotBeNullErrorException();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateListenEndPointNotBeNullErrorException() => new ListenEndPointNotBeNullException();
    }
}