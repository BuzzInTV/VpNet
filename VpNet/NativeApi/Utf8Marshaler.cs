using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VpNet.NativeApi
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
            var utf32Data = Encoding.UTF8.GetBytes((string)managedObj);
            var buffer = Marshal.AllocHGlobal(utf32Data.Length + 1);
            Marshal.Copy(utf32Data, 0, buffer, utf32Data.Length);
            Marshal.WriteByte(buffer, utf32Data.Length, 0);
            return buffer;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class Utf8StringToManaged : ICustomMarshaler
    {
        private static Utf8StringToManaged s_instance;

        public static ICustomMarshaler GetInstance(string cookie)
        {
            if (s_instance == null)
            {
                s_instance = new Utf8StringToManaged();
            }

            return s_instance;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            // Do nothing. For some reason CleanUpNativeData is called for pointers that are not even created by the marshaler.
            // This can cause heap corruption because of double free or because a different allocator may have been used to
            // allocate the memory block.
        }

        public void CleanUpManagedData(object managedObj)
        {
        }

        public int GetNativeDataSize()
        {
            return -1;
        }

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            throw new NotImplementedException();
        }

        private int GetStringLength(IntPtr ptr)
        {
            int offset;
            for (offset = 0; Marshal.ReadByte(ptr, offset) != 0; offset++)
            {
            }

            return offset;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
            {
                return null;
            }

            var buffer = new byte[GetStringLength(pNativeData)];
            Marshal.Copy(pNativeData, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
