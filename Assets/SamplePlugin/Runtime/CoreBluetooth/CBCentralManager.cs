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
        NativeCentralManagerProxy nativeCentralManagerProxy;
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
            instance.nativeCentralManagerProxy = new NativeCentralManagerProxy(instance.handle);
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

        CBCentralManagerDelegate _centralManagerDelegate;
        public CBCentralManagerDelegate centralManagerDelegate
        {
            get => _centralManagerDelegate;
            set
            {
                ExceptionUtils.ThrowObjectDisposedExceptionIf(disposed, this);
                _centralManagerDelegate = value;
            }
        }

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

        public CBPeripheral[] RetrievePeripheralsWithIdentifiers(string[] identifiers)
        {
            ExceptionUtils.ThrowObjectDisposedExceptionIf(disposed, this);
            var foundPeripherals = nativeCentralManagerProxy.RetrievePeripheralsWithIdentifiers(identifiers)
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
            ExceptionUtils.ThrowObjectDisposedExceptionIf(disposed, this);
            nativeCentralManagerProxy.ScanForPeripherals(serviceUUIDs);
        }

        public void StopScan()
        {
            ExceptionUtils.ThrowObjectDisposedExceptionIf(disposed, this);
            nativeCentralManagerProxy.StopScan();
        }

        public bool isScanning
        {
            get
            {
                ExceptionUtils.ThrowObjectDisposedExceptionIf(disposed, this);
                return nativeCentralManagerProxy.IsScanning();
            }
        }

        #endregion

        #region Establishing or Canceling Connections with Peripherals

        // NOTE: options is not implemented yet.
        // https://developer.apple.com/documentation/corebluetooth/cbcentralmanager/1518766-connect
        public void Connect(CBPeripheral peripheral)
        {
            ExceptionUtils.ThrowObjectDisposedExceptionIf(disposed, this);
            if (peripheral.state != CBPeripheralState.disconnected)
            {
                UnityEngine.Debug.LogWarning("peripheral.state is not disconnected.");
            }

            nativeCentralManagerProxy.Connect(peripheral.identifier);
        }

        public void CancelPeripheralConnection(CBPeripheral peripheral)
        {
            ExceptionUtils.ThrowObjectDisposedExceptionIf(disposed, this);
            nativeCentralManagerProxy.CancelPeripheralConnection(peripheral.identifier);
        }

        #endregion

        CBPeripheral GetPeripheral(IntPtr peripheralIdPtr)
        {
            if (!peripherals.TryGetValue(Marshal.PtrToStringUTF8(peripheralIdPtr), out var peripheral))
            {
                UnityEngine.Debug.LogError("Peripheral not found.");
                return null;
            }
            return peripheral;
        }

        CBCharacteristic FindCharacteristic(CBPeripheral peripheral, IntPtr serviceIdPtr, IntPtr characteristicIdPtr)
        {
            return peripheral.FindCharacteristic(
                Marshal.PtrToStringUTF8(serviceIdPtr),
                Marshal.PtrToStringUTF8(characteristicIdPtr)
            );
        }

        internal void OnDidUpdateState(CBManagerState state)
        {
            if (disposed) return;
            this.state = state;
            centralManagerDelegate?.CentralManagerDidUpdateState(this);
        }

        internal void OnDidDiscoverPeripheral(IntPtr peripheralIdPtr, IntPtr peripheralNamePtr)
        {
            if (disposed) return;
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
            if (disposed) return;
            var peripheral = GetPeripheral(peripheralIdPtr);
            if (peripheral == null) return;
            centralManagerDelegate?.CentralManagerDidConnectPeripheral(this, peripheral);
        }

        internal void OnDidFailToConnectPeripheral(IntPtr peripheralIdPtr, int errorCode)
        {
            if (disposed) return;
            var peripheral = GetPeripheral(peripheralIdPtr);
            if (peripheral == null) return;
            centralManagerDelegate?.CentralManagerDidFailToConnectPeripheral(this, peripheral, CBError.CreateOrNullFromCode(errorCode));
        }

        internal void OnDidDisconnectPeripheral(IntPtr peripheralIdPtr, int errorCode)
        {
            if (disposed) return;
            var peripheral = GetPeripheral(peripheralIdPtr);
            if (peripheral == null) return;
            centralManagerDelegate?.CentralManagerDidDisconnectPeripheral(this, peripheral, CBError.CreateOrNullFromCode(errorCode));
        }

        internal void OnDidDiscoverServices(IntPtr peripheralIdPtr, IntPtr commaSeparatedServiceIdsPtr, int errorCode)
        {
            if (disposed) return;
            var peripheral = GetPeripheral(peripheralIdPtr);
            if (peripheral == null) return;
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
            if (disposed) return;
            var peripheral = GetPeripheral(peripheralIdPtr);
            if (peripheral == null) return;

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
            if (disposed) return;
            var peripheral = GetPeripheral(peripheralIdPtr);
            if (peripheral == null) return;

            var characteristic = FindCharacteristic(peripheral, serviceIdPtr, characteristicIdPtr);
            var valueBytes = new byte[valueLength];
            Marshal.Copy(valuePtr, valueBytes, 0, valueLength);
            characteristic.SetValue(valueBytes);
            peripheral.OnDidUpdateValueForCharacteristic(characteristic, CBError.CreateOrNullFromCode(errorCode));
        }

        internal void OnDidWriteValueForCharacteristic(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, int errorCode)
        {
            if (disposed) return;
            var peripheral = GetPeripheral(peripheralIdPtr);
            if (peripheral == null) return;
            var characteristic = FindCharacteristic(peripheral, serviceIdPtr, characteristicIdPtr);
            peripheral.OnDidWriteValueForCharacteristic(characteristic, CBError.CreateOrNullFromCode(errorCode));
        }

        internal void OnDidUpdateNotificationStateForCharacteristic(IntPtr peripheralIdPtr, IntPtr serviceIdPtr, IntPtr characteristicIdPtr, int notificationState, int errorCode)
        {
            if (disposed) return;
            var peripheral = GetPeripheral(peripheralIdPtr);
            if (peripheral == null) return;
            var characteristic = FindCharacteristic(peripheral, serviceIdPtr, characteristicIdPtr);
            bool isNotifying = notificationState == 1;
            peripheral.OnDidUpdateNotificationStateForCharacteristic(characteristic, isNotifying, CBError.CreateOrNullFromCode(errorCode));
        }

        internal void OnDidReadRSSI(IntPtr peripheralIdPtr, int rssi, int errorCode)
        {
            if (disposed) return;
            GetPeripheral(peripheralIdPtr)?.OnDidReadRSSI(rssi, CBError.CreateOrNullFromCode(errorCode));
        }
    }
}
