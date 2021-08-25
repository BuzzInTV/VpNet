﻿using System;
using System.Runtime.InteropServices;

namespace VpNet.Internal
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int SocketSendFunction(IntPtr socket, IntPtr data, uint length);
}