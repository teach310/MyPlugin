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
        internal static extern void cb4u_central_manager_release(IntPtr centralPtr);

        internal delegate void CB4UCentralManagerDidUpdateStateHandler(IntPtr centralPtr, CBManagerState state);
        internal delegate void CB4UCentralManagerDidDiscoverPeripheralHandler(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr peripheralNamePtr);
        internal delegate void CB4UCentralManagerDidConnectPeripheralHandler(IntPtr centralPtr, IntPtr peripheralIdPtr);
        internal delegate void CB4UCentralManagerDidFailToConnectPeripheralHandler(IntPtr centralPtr, IntPtr peripheralIdPtr, int errorCode);
        internal delegate void CB4UCentralManagerDidDisconnectPeripheralHandler(IntPtr centralPtr, IntPtr peripheralIdPtr, int errorCode);

        // NOTE: use comma separated service ids to avoid to use array of string
        internal delegate void CB4UPeripheralDidDiscoverServicesHandler(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr commaSeparatedServiceIdsPtr, int errorCode);
        internal delegate void CB4UPeripheralDidDiscoverCharacteristicsHandler(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr commaSeparatedCharacteristicIdsPtr, int errorCode);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cb4u_central_manager_register_handlers(
            IntPtr centralPtr,
            CB4UCentralManagerDidUpdateStateHandler didUpdateStateHandler,
            CB4UCentralManagerDidDiscoverPeripheralHandler didDiscoverPeripheralHandler,
            CB4UCentralManagerDidConnectPeripheralHandler didConnectPeripheralHandler,
            CB4UCentralManagerDidFailToConnectPeripheralHandler didFailToConnectPeripheralHandler,
            CB4UCentralManagerDidDisconnectPeripheralHandler didDisconnectPeripheralHandler,
            CB4UPeripheralDidDiscoverServicesHandler didDiscoverServicesHandler,
            CB4UPeripheralDidDiscoverCharacteristicsHandler didDiscoverCharacteristicsHandler
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cb4u_central_manager_scan_for_peripherals(
            IntPtr centralPtr,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 2)] string[] serviceUUIDs,
            int serviceUUIDsCount
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cb4u_central_manager_stop_scan(IntPtr centralPtr);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool cb4u_central_manager_is_scanning(IntPtr centralPtr);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int cb4u_central_manager_connect_peripheral(IntPtr centralPtr, [MarshalAs(UnmanagedType.LPStr), In] string peripheralId);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int cb4u_peripheral_discover_services(IntPtr centralPtr, [MarshalAs(UnmanagedType.LPStr), In] string peripheralId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 3)] string[] serviceUUIDs, int serviceUUIDsCount);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int cb4u_peripheral_discover_characteristics(IntPtr centralPtr, [MarshalAs(UnmanagedType.LPStr), In] string peripheralId, [MarshalAs(UnmanagedType.LPStr), In] string serviceId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] characteristicUUIDs, int characteristicUUIDsCount);
    }
}
