using System;
using System.Runtime.InteropServices;

namespace CoreBluetooth
{
    internal static partial class NativeMethods
    {
#if UNITY_IOS && !UNITY_EDITOR
        const string DLL_NAME = "__Internal";
#else
        const string DLL_NAME = "SamplePluginBundle";
#endif

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr cb4u_central_manager_new();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cb4u_central_manager_release(IntPtr instancePtr);

        internal delegate void CB4UCentralManagerDidUpdateStateHandler(IntPtr instancePtr, CBManagerState state);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cb4u_central_manager_register_handlers(
            IntPtr instancePtr,
            CB4UCentralManagerDidUpdateStateHandler didUpdateStateHandler
        );
    }
}