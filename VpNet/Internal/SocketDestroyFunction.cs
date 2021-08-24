using System;
using System.Runtime.InteropServices;

namespace VpNet.Internal
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void SocketDestroyFunction(IntPtr socket);
}