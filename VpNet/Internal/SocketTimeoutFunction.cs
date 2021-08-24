using System;
using System.Runtime.InteropServices;

namespace VpNet.Internal
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int SocketTimeoutFunction(IntPtr socket, int seconds);
}