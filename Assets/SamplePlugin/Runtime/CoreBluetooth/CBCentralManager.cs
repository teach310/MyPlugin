using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbcentralmanager
    public class CBCentralManager : IDisposable
    {
        bool disposed = false;

        SafeCB4UCentralManagerHandle handle;
        // key: peripheralId, value: CBPeripheral
        Dictionary<string, CBPeripheral> peripherals = new Dictionary<string, CBPeripheral>();
        NativePeripheralProxy nativePeripheralProxy;
        NativeCharacteristicProxy nativeCharacteristicProxy;

        CBCentralManager() { }

        ~CBCentralManager() => Dispose(false);

        // NOTE: options is not implemented yet.
        // https://developer.apple.com/documentation/corebluetooth/cbcentralmanager/1519001-init
        public static CBCentralManager Create(CBCentralManagerDelegate centralManagerDelegate = null)
        {
            var instance = new CBCentralManager();
            instance.handle = SafeCB4UCentralManagerHandle.Create(instance);
            instance.nativePeripheralProxy = new NativePeripheralProxy(instance.handle);
            instance.nativeCharacteristicProxy = new NativeCharacteristicProxy(instance.handle);
            instance.centralManagerDelegate = centralManagerDelegate;
            return instance;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
            }
        }

        public CBManagerState state { get; private set; } = CBManagerState.unknown;
        public CBCentralManagerDelegate centralManagerDelegate { get; set; }

        CBPeripheral GetOrCreatePeripheral(string peripheralId)
        {
            if (peripherals.TryGetValue(peripheralId, out var peripheral))
            {
                return peripheral;
            }
            else
            {
                return new CBPeripheral(peripheralId, nativePeripheralProxy.Name(peripheralId), nativePeripheralProxy);
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

        internal void OnDidUpdateState(CBManagerState state)
        {
            this.state = state;
            centralManagerDelegate?.CentralManagerDidUpdateState(this);
        }

        internal void OnDidDiscoverPeripheral(IntPtr peripheralIdPtr, IntPtr peripheralNamePtr)
        {
            var peripheral = new CBPeripheral(
                Marshal.PtrToStringUTF8(peripheralIdPtr),
                Marshal.PtrToStringUTF8(peripheralNamePtr),
                nativePeripheralProxy
            );

            peripherals[peripheral.identifier] = peripheral;
            centralManagerDelegate?.CentralManagerDidDiscoverPeripheral(this, peripheral);
        }

        internal void OnDidConnectPeripheral(IntPtr peripheralIdPtr)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

            centralManagerDelegate?.CentralManagerDidConnectPeripheral(this, peripheral);
        }

        internal void OnDidFailToConnectPeripheral(IntPtr peripheralIdPtr, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

            centralManagerDelegate?.CentralManagerDidFailToConnectPeripheral(this, peripheral, CBError.CreateOrNullFromCode(errorCode));
        }

        internal void OnDidDisconnectPeripheral(IntPtr peripheralIdPtr, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }

            centralManagerDelegate?.CentralManagerDidDisconnectPeripheral(this, peripheral, CBError.CreateOrNullFromCode(errorCode));
        }

        internal void OnDidDiscoverServices(IntPtr peripheralIdPtr, IntPtr commaSeparatedServiceIdsPtr, int errorCode)
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

        internal void OnDidDiscoverCharacteristics(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr commaSeparatedCharacteristicIdsPtr, int errorCode)
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

            var characteristics = characteristicIds.Select(characteristicId => new CBCharacteristic(characteristicId, service, nativeCharacteristicProxy)).ToArray();
            peripheral.OnDidDiscoverCharacteristics(characteristics, service, CBError.CreateOrNullFromCode(errorCode));
        }

        internal void OnDidUpdateValueForCharacteristic(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, IntPtr valuePtr, int valueLength, int errorCode)
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

        internal void OnDidWriteValueForCharacteristic(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, int errorCode)
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

        internal void OnDidUpdateNotificationStateForCharacteristic(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, int notificationState, int errorCode)
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

        internal void OnDidReadRSSI(IntPtr peripheralIdPtr, int rssi, int errorCode)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return;
            }
            peripheral.OnDidReadRSSI(rssi, CBError.CreateOrNullFromCode(errorCode));
        }
    }
}
