﻿using System;
using System.Runtime.InteropServices;
using VpNet.NativeApi;

namespace VpNet.Internal
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NetConfig
    {
        public SocketCreateFunction Create;
        public SocketDestroyFunction Destroy;
        public SocketConnectFunction Connect;
        public SocketSendFunction Send;
        public SocketReceiveFunction Receive;
        public SocketTimeoutFunction Timeout;
        public SocketWaitFunction Wait;
        public IntPtr Context;
    }
}