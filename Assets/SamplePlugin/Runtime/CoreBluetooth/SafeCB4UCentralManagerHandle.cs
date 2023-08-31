using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CoreBluetooth
{
    internal class SafeCB4UCentralManagerHandle : SafeHandle
    {
        static Dictionary<IntPtr, CBCentralManager> centralManagerMap = new Dictionary<IntPtr, CBCentralManager>();
        SafeCB4UCentralManagerHandle(IntPtr handle) : base(handle, true) { }

        public override bool IsInvalid => handle == IntPtr.Zero;

        internal static SafeCB4UCentralManagerHandle Create(CBCentralManager centralManager)
        {
            var handle = NativeMethods.cb4u_central_manager_new();
            var instance = new SafeCB4UCentralManagerHandle(handle);
            RegisterHandlers(instance);
            centralManagerMap.Add(handle, centralManager);
            return instance;
        }

        internal static bool TryGetCBCentralManager(IntPtr handle, out CBCentralManager centralManager)
        {
            return centralManagerMap.TryGetValue(handle, out centralManager);
        }

        static void CallInstanceMethod(IntPtr centralPtr, Action<CBCentralManager> action)
        {
            if (!SafeCB4UCentralManagerHandle.TryGetCBCentralManager(centralPtr, out var instance))
            {
                UnityEngine.Debug.LogError("CBCentralManager instance not found.");
                return;
            }

            action(instance);
        }

        static void RegisterHandlers(SafeCB4UCentralManagerHandle handle)
        {
            NativeMethods.cb4u_central_manager_register_handlers(
                handle,
                OnDidUpdateState,
                OnDidDiscoverPeripheral,
                OnDidConnectPeripheral,
                OnDidFailToConnectPeripheral,
                OnDidDisconnectPeripheral,
                OnDidDiscoverServices,
                OnDidDiscoverCharacteristics,
                OnDidUpdateValueForCharacteristic,
                OnDidWriteValueForCharacteristic,
                OnDidUpdateNotificationStateForCharacteristic,
                OnDidReadRSSI
            );
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UCentralManagerDidUpdateStateHandler))]
        internal static void OnDidUpdateState(IntPtr centralPtr, CBManagerState state)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidUpdateState(state));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UCentralManagerDidDiscoverPeripheralHandler))]
        internal static void OnDidDiscoverPeripheral(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr peripheralNamePtr)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidDiscoverPeripheral(peripheralIdPtr, peripheralNamePtr));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UCentralManagerDidConnectPeripheralHandler))]
        internal static void OnDidConnectPeripheral(IntPtr centralPtr, IntPtr peripheralIdPtr)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidConnectPeripheral(peripheralIdPtr));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UCentralManagerDidFailToConnectPeripheralHandler))]
        internal static void OnDidFailToConnectPeripheral(IntPtr centralPtr, IntPtr peripheralIdPtr, int errorCode)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidFailToConnectPeripheral(peripheralIdPtr, errorCode));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UCentralManagerDidDisconnectPeripheralHandler))]
        internal static void OnDidDisconnectPeripheral(IntPtr centralPtr, IntPtr peripheralIdPtr, int errorCode)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidDisconnectPeripheral(peripheralIdPtr, errorCode));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UPeripheralDidDiscoverServicesHandler))]
        internal static void OnDidDiscoverServices(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr commaSeparatedServiceIdsPtr, int errorCode)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidDiscoverServices(peripheralIdPtr, commaSeparatedServiceIdsPtr, errorCode));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UPeripheralDidDiscoverCharacteristicsHandler))]
        internal static void OnDidDiscoverCharacteristics(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr commaSeparatedCharacteristicIdsPtr, int errorCode)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidDiscoverCharacteristics(peripheralIdPtr, serviceIdPtr, commaSeparatedCharacteristicIdsPtr, errorCode));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UPeripheralDidUpdateValueForCharacteristicHandler))]
        internal static void OnDidUpdateValueForCharacteristic(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, IntPtr valuePtr, int valueLength, int errorCode)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidUpdateValueForCharacteristic(peripheralIdPtr, serviceIdPtr, characteristicIdPtr, valuePtr, valueLength, errorCode));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UPeripheralDidWriteValueForCharacteristicHandler))]
        internal static void OnDidWriteValueForCharacteristic(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, int errorCode)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidWriteValueForCharacteristic(peripheralIdPtr, serviceIdPtr, characteristicIdPtr, errorCode));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UPeripheralDidUpdateNotificationStateForCharacteristicHandler))]
        internal static void OnDidUpdateNotificationStateForCharacteristic(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, int notificationState, int errorCode)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidUpdateNotificationStateForCharacteristic(peripheralIdPtr, serviceIdPtr, characteristicIdPtr, notificationState, errorCode));
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UPeripheralDidReadRSSIHandler))]
        internal static void OnDidReadRSSI(IntPtr centralPtr, IntPtr peripheralIdPtr, int rssi, int errorCode)
        {
            CallInstanceMethod(centralPtr, instance => instance.OnDidReadRSSI(peripheralIdPtr, rssi, errorCode));
        }

        protected override bool ReleaseHandle()
        {
            centralManagerMap.Remove(handle);
            NativeMethods.cb4u_central_manager_release(handle);
            return true;
        }
    }
}
