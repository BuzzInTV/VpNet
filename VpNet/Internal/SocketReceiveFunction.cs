using System;
using System.Runtime.InteropServices;

namespace VpNet.Internal
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SocketReceiveFunction(IntPtr socket, IntPtr data, uint length);
}