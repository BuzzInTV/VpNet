using System;
using System.Runtime.InteropServices;

namespace VpNet.Internal
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int SocketConnectFunction(
        IntPtr socket,
        IntPtr host, //[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8StringToNative))] string host, 
        ushort port);
}