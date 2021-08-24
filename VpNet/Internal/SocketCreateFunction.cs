using System;
using System.Runtime.InteropServices;

namespace VpNet.Internal
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr SocketCreateFunction(IntPtr connection, IntPtr context);
}
