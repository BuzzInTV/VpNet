using System;
using System.Runtime.InteropServices;

namespace VpNet.Internal
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr SocketCreateFunction(IntPtr connection, IntPtr context);
}
