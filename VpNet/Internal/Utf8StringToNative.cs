﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VpNet.Internal
{
    internal sealed class Utf8StringToNative : ICustomMarshaler
    {
        private static Utf8StringToNative s_instance;

        public static ICustomMarshaler GetInstance(string cookie)
        {
            if (s_instance == null)
            {
                s_instance = new Utf8StringToNative();
            }

            return s_instance;
        }

        public void CleanUpManagedData(object managedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeHGlobal(pNativeData);
        }

        public int GetNativeDataSize()
        {
            return -1;
        }

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            byte[] utf8Data = Encoding.UTF8.GetBytes((string)managedObj);
            IntPtr buffer = Marshal.AllocHGlobal(utf8Data.Length + 1);
            Marshal.Copy(utf8Data, 0, buffer, utf8Data.Length);
            Marshal.WriteByte(buffer, utf8Data.Length, 0);
            return buffer;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            throw new NotImplementedException();
        }
    }
}