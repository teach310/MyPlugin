using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbcentralmanager
    public class CBCentralManager : SafeHandle
    {
        static readonly Dictionary<IntPtr, CBCentralManager> instanceMap = new Dictionary<IntPtr, CBCentralManager>();
        // key: peripheralId, value: CBPeripheral
        Dictionary<string, CBPeripheral> peripherals = new Dictionary<string, CBPeripheral>();

        public override bool IsInvalid => handle == IntPtr.Zero;

        CBCentralManager(IntPtr handle) : base(IntPtr.Zero, true)
        {
            this.handle = handle;
            instanceMap.Add(handle, this);
        }

        protected override bool ReleaseHandle()
        {
            instanceMap.Remove(handle);
            NativeMethods.cb4u_central_manager_release(handle);
            return true;
        }

        public CBManagerState state { get; private set; } = CBManagerState.unknown;
        public CBCentralManagerDelegate centralManagerDelegate { get; set; }

        #region Scanning or Stopping Scans of Peripherals

        public void ScanForPeripherals(string[] serviceUUIDs)
        {
            NativeMethods.cb4u_central_manager_scan_for_peripherals(
                handle,
                serviceUUIDs,
                serviceUUIDs.Length
            );
        }

        public void StopScan() => NativeMethods.cb4u_central_manager_stop_scan(handle);
        public bool isScanning => NativeMethods.cb4u_central_manager_is_scanning(handle);

        #endregion

        #region Establishing or Canceling Connections with Peripherals

        // NOTE: options is not implemented yet.
        // https://developer.apple.com/documentation/corebluetooth/cbcentralmanager/1518766-connect
        public void Connect(CBPeripheral peripheral)
        {
            if (peripheral.state != CBPeripheralState.disconnected)
            {
                UnityEngine.Debug.LogWarning("peripheral.state is not disconnected.");
            }

            peripheral.SetState(CBPeripheralState.connecting);
            var result = NativeMethods.cb4u_central_manager_connect_peripheral(handle, peripheral.identifier);
            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute connect.");
            }
        }

        #endregion

        internal void DiscoverServices(CBPeripheral peripheral, string[] serviceUUIDs)
        {
            int result = NativeMethods.cb4u_peripheral_discover_services(
                handle,
                peripheral.identifier,
                serviceUUIDs,
                serviceUUIDs.Length
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute discover services.");
            }
        }

        internal void DiscoverCharacteristics(CBPeripheral peripheral, CBService service, string[] characteristicUUIDs)
        {
            int result = NativeMethods.cb4u_peripheral_discover_characteristics(
                handle,
                peripheral.identifier,
                service.identifier,
                characteristicUUIDs,
                characteristicUUIDs.Length
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute discover characteristics.");
            }
        }

        // NOTE: options is not implemented yet.
        // https://developer.apple.com/documentation/corebluetooth/cbcentralmanager/1519001-init
        public static CBCentralManager Create(CBCentralManagerDelegate centralManagerDelegate = null)
        {
            var handle = NativeMethods.cb4u_central_manager_new();
            var instance = new CBCentralManager(handle);
            instance.centralManagerDelegate = centralManagerDelegate;
            CentralEventHandler.Register(handle);
            return instance;
        }

        void OnDidUpdateState(CBManagerState state)
        {
            this.state = state;
            centralManagerDelegate?.CentralManagerDidUpdateState(this);
        }

        void OnDidDiscoverPeripheral(IntPtr peripheralIdPtr, IntPtr peripheralNamePtr)
        {
            var peripheral = new CBPeripheral(
                this,
                Marshal.PtrToStringUTF8(peripheralIdPtr),
                Marshal.PtrToStringUTF8(peripheralNamePtr)
            );

            peripherals[peripheral.identifier] = peripheral;
            centralManagerDelegate?.CentralManagerDidDiscoverPeripheral(this, peripheral);
        }

        void OnDidConnectPeripheral(IntPtr peripheralIdPtr)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }
            peripheral.SetState(CBPeripheralState.connected);
            centralManagerDelegate?.CentralManagerDidConnectPeripheral(this, peripheral);
        }

        void OnDidFailToConnectPeripheral(IntPtr peripheralIdPtr, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }
            peripheral.SetState(CBPeripheralState.disconnected);
            centralManagerDelegate?.CentralManagerDidFailToConnectPeripheral(this, peripheral, CBError.CreateOrNullFromCode(errorCode));
        }

        void OnDidDisconnectPeripheral(IntPtr peripheralIdPtr, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }
            peripheral.SetState(CBPeripheralState.disconnected);
            centralManagerDelegate?.CentralManagerDidDisconnectPeripheral(this, peripheral, CBError.CreateOrNullFromCode(errorCode));
        }

        void OnDidDiscoverServices(IntPtr peripheralIdPtr, IntPtr commaSeparatedServiceIdsPtr, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

            string commaSeparatedServiceIds = Marshal.PtrToStringUTF8(commaSeparatedServiceIdsPtr);
            if (string.IsNullOrEmpty(commaSeparatedServiceIds))
            {
                UnityEngine.Debug.LogError("OnDidDiscoverService is called with empty serviceIds.");
                return;
            }

            var serviceIds = Marshal.PtrToStringUTF8(commaSeparatedServiceIdsPtr).Split(',').ToList();
            // NOTE: get service info here if needed.
            var services = serviceIds.Select(serviceId => new CBService(serviceId, peripheral)).ToArray();
            peripheral.OnDidDiscoverServices(services, CBError.CreateOrNullFromCode(errorCode));

        }

        void OnDidDiscoverCharacteristics(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr commaSeparatedCharacteristicIdsPtr, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

            string commaSeparatedCharacteristicIds = Marshal.PtrToStringUTF8(commaSeparatedCharacteristicIdsPtr);
            if (string.IsNullOrEmpty(commaSeparatedCharacteristicIds))
            {
                UnityEngine.Debug.LogError("OnDidDiscoverCharacteristics is called with empty characteristicIds.");
                return;
            }

            var characteristicIds = Marshal.PtrToStringUTF8(commaSeparatedCharacteristicIdsPtr).Split(',').ToList();

            var service = peripheral.services.FirstOrDefault(s => s.identifier == Marshal.PtrToStringUTF8(serviceIdPtr));
            if (service == null)
            {
                UnityEngine.Debug.LogError("Service not found.");
                return;
            }

            // NOTE: get characteristic info here if needed.
            var characteristics = characteristicIds.Select(characteristicId => new CBCharacteristic(characteristicId, service)).ToArray();
            peripheral.OnDidDiscoverCharacteristics(characteristics, service, CBError.CreateOrNullFromCode(errorCode));
        }

        static class CentralEventHandler
        {
            internal static void Register(IntPtr centralPtr)
            {
                NativeMethods.cb4u_central_manager_register_handlers(
                    centralPtr,
                    OnDidUpdateState,
                    OnDidDiscoverPeripheral,
                    OnDidConnectPeripheral,
                    OnDidFailToConnectPeripheral,
                    OnDidDisconnectPeripheral,
                    OnDidDiscoverServices,
                    OnDidDiscoverCharacteristics
                );
            }

            static void CallInstanceMethod(IntPtr centralPtr, Action<CBCentralManager> action)
            {
                if (!instanceMap.TryGetValue(centralPtr, out var instance))
                {
                    UnityEngine.Debug.LogError("CBCentralManager instance not found.");
                    return;
                }

                action(instance);
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
        }
    }
}
