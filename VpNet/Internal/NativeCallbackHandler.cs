using System;
using System.Runtime.InteropServices;
using VpNet.NativeApi;

namespace VpNet.Internal
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void NativeCallbackHandler(IntPtr sender, [MarshalAs(UnmanagedType.I4)] ReasonCode reason, int reference);
}
