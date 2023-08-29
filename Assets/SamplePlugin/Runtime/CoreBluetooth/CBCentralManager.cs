using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbcentralmanager
    public class CBCentralManager : SafeHandle, ICharacteristicNativeMethods, IPeripheralNativeMethods
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

        string GetPeripheralName(string peripheralId)
        {
            var sb = new StringBuilder(256);
            var result = NativeMethods.cb4u_central_manager_peripheral_name(
                handle,
                peripheralId,
                sb,
                sb.Capacity
            );

            if (result < 0)
            {
                return string.Empty;
            }

            return sb.ToString();
        }

        CBPeripheral GetOrCreatePeripheral(string peripheralId)
        {
            if (peripherals.TryGetValue(peripheralId, out var peripheral))
            {
                return peripheral;
            }
            else
            {
                return new CBPeripheral(peripheralId, GetPeripheralName(peripheralId), this);
            }
        }

        public CBPeripheral[] RetrievePeripheralsWithIdentifiers(string[] peripheralUUIDs)
        {
            var sbSize = peripheralUUIDs.Length * (36 + 1) + 1; // 36 is the length of UUID string, 1 is for comma.
            var sb = new StringBuilder(sbSize);
            var result = NativeMethods.cb4u_central_manager_retrieve_peripherals_with_identifiers(
                handle,
                peripheralUUIDs,
                peripheralUUIDs.Length,
                sb,
                sbSize
            );

            if (result < 0)
            {
                return new CBPeripheral[0];
            }

            var commaSeparatedPeripheralUUIDs = sb.ToString();
            var foundPeripherals = commaSeparatedPeripheralUUIDs
                .Split(',')
                .Select(uuid => GetOrCreatePeripheral(uuid))
                .ToArray();
            foreach (var peripheral in foundPeripherals)
            {
                peripherals[peripheral.identifier] = peripheral;
            }
            return foundPeripherals;
        }

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

            var result = NativeMethods.cb4u_central_manager_connect_peripheral(handle, peripheral.identifier);
            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute connect.");
            }
        }

        public void CancelPeripheralConnection(CBPeripheral peripheral)
        {
            int result = NativeMethods.cb4u_central_manager_cancel_peripheral_connection(handle, peripheral.identifier);
            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute cancel peripheral connection.");
            }
        }

        #endregion

        void IPeripheralNativeMethods.DiscoverServices(CBPeripheral peripheral, string[] serviceUUIDs)
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

        void IPeripheralNativeMethods.DiscoverCharacteristics(CBPeripheral peripheral, CBService service, string[] characteristicUUIDs)
        {
            int result = NativeMethods.cb4u_peripheral_discover_characteristics(
                handle,
                peripheral.identifier,
                service.uuid,
                characteristicUUIDs,
                characteristicUUIDs.Length
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute discover characteristics.");
            }
        }

        void IPeripheralNativeMethods.ReadValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic)
        {
            int result = NativeMethods.cb4u_peripheral_read_value_for_characteristic(
                handle,
                peripheral.identifier,
                characteristic.service.uuid,
                characteristic.uuid
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute read value for characteristic.");
            }
        }

        void IPeripheralNativeMethods.WriteValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic, byte[] data, CBCharacteristicWriteType type)
        {
            int result = NativeMethods.cb4u_peripheral_write_value_for_characteristic(
                handle,
                peripheral.identifier,
                characteristic.service.uuid,
                characteristic.uuid,
                data,
                data.Length,
                (int)type
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute write value for characteristic.");
            }
        }

        void IPeripheralNativeMethods.SetNotifyValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic, bool enabled)
        {
            int result = NativeMethods.cb4u_peripheral_set_notify_value_for_characteristic(
                handle,
                peripheral.identifier,
                characteristic.service.uuid,
                characteristic.uuid,
                enabled
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute set notify value for characteristic.");
            }
        }

        void IPeripheralNativeMethods.ReadRSSI(CBPeripheral peripheral)
        {
            int result = NativeMethods.cb4u_central_manager_peripheral_read_rssi(
                handle,
                peripheral.identifier
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute read RSSI.");
            }
        }

        CBPeripheralState IPeripheralNativeMethods.GetPeripheralState(CBPeripheral peripheral)
        {
            var stateInt = NativeMethods.cb4u_central_manager_peripheral_state(handle,peripheral.identifier);

            if (stateInt < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute get peripheral state.");
            }
            return (CBPeripheralState)stateInt;
        }

        CBCharacteristicProperties ICharacteristicNativeMethods.GetCharacteristicProperties(CBCharacteristic characteristic)
        {
            var propertiesInt = NativeMethods.cb4u_central_manager_characteristic_properties(
                handle,
                characteristic.service.peripheral.identifier,
                characteristic.service.uuid,
                characteristic.uuid
            );

            if (propertiesInt < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute get characteristic properties.");
            }
            return (CBCharacteristicProperties)propertiesInt;
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
                Marshal.PtrToStringUTF8(peripheralIdPtr),
                Marshal.PtrToStringUTF8(peripheralNamePtr),
                this
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

            centralManagerDelegate?.CentralManagerDidConnectPeripheral(this, peripheral);
        }

        void OnDidFailToConnectPeripheral(IntPtr peripheralIdPtr, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

            centralManagerDelegate?.CentralManagerDidFailToConnectPeripheral(this, peripheral, CBError.CreateOrNullFromCode(errorCode));
        }

        void OnDidDisconnectPeripheral(IntPtr peripheralIdPtr, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

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

            var service = peripheral.services.FirstOrDefault(s => s.uuid == Marshal.PtrToStringUTF8(serviceIdPtr));
            if (service == null)
            {
                UnityEngine.Debug.LogError("Service not found.");
                return;
            }

            // NOTE: get characteristic info here if needed.
            var characteristics = characteristicIds.Select(characteristicId => new CBCharacteristic(characteristicId, service, this)).ToArray();
            peripheral.OnDidDiscoverCharacteristics(characteristics, service, CBError.CreateOrNullFromCode(errorCode));
        }

        void OnDidUpdateValueForCharacteristic(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, IntPtr valuePtr, int valueLength, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

            var characteristic = peripheral.FindCharacteristic(
                Marshal.PtrToStringUTF8(serviceIdPtr),
                Marshal.PtrToStringUTF8(characteristicIdPtr)
            );
            var valueBytes = new byte[valueLength];
            Marshal.Copy(valuePtr, valueBytes, 0, valueLength);
            characteristic.SetValue(valueBytes);
            peripheral.OnDidUpdateValueForCharacteristic(characteristic, CBError.CreateOrNullFromCode(errorCode));
        }

        void OnDidWriteValueForCharacteristic(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

            var characteristic = peripheral.FindCharacteristic(
                Marshal.PtrToStringUTF8(serviceIdPtr),
                Marshal.PtrToStringUTF8(characteristicIdPtr)
            );
            peripheral.OnDidWriteValueForCharacteristic(characteristic, CBError.CreateOrNullFromCode(errorCode));
        }

        void OnDidUpdateNotificationStateForCharacteristic(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, int notificationState, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

            var characteristic = peripheral.FindCharacteristic(
                Marshal.PtrToStringUTF8(serviceIdPtr),
                Marshal.PtrToStringUTF8(characteristicIdPtr)
            );

            bool isNotifying = notificationState == 1;
            peripheral.OnDidUpdateNotificationStateForCharacteristic(characteristic, isNotifying, CBError.CreateOrNullFromCode(errorCode));
        }

        void OnDidReadRSSI(IntPtr peripheralIdPtr, int rssi, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }
            peripheral.OnDidReadRSSI(rssi, CBError.CreateOrNullFromCode(errorCode));
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
                    OnDidDiscoverCharacteristics,
                    OnDidUpdateValueForCharacteristic,
                    OnDidWriteValueForCharacteristic,
                    OnDidUpdateNotificationStateForCharacteristic,
                    OnDidReadRSSI
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
        }
    }
}
