using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CoreBluetooth
{
    internal interface IPeripheralNativeMethods
    {
        void DiscoverServices(CBPeripheral peripheral, string[] serviceUUIDs);
        void DiscoverCharacteristics(CBPeripheral peripheral, CBService service, string[] characteristicUUIDs);
        void ReadValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic);
        void WriteValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic, byte[] data, CBCharacteristicWriteType type);
        void SetNotifyValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic, bool enabled);
        void ReadRSSI(CBPeripheral peripheral);
        CBPeripheralState GetPeripheralState(CBPeripheral peripheral);
    }

    // https://developer.apple.com/documentation/corebluetooth/cbperipheral
    public class CBPeripheral
    {
        IPeripheralNativeMethods nativeMethods;
        public string identifier { get; }
        public string name { get; }
        public CBPeripheralState state => nativeMethods.GetPeripheralState(this);
        public CBPeripheralDelegate peripheralDelegate { get; set; }
        List<CBService> _services = new List<CBService>();
        public ReadOnlyCollection<CBService> services { get; }

        internal CBPeripheral(string id, string name, IPeripheralNativeMethods nativeMethods)
        {
            this.identifier = id;
            this.name = name;
            this.nativeMethods = nativeMethods;
            this.services = _services.AsReadOnly();
        }

        // TODO: output mtu
        public override string ToString()
        {
            return $"CBPeripheral: identifier = {identifier}, name = {name}, state = {state}";
        }

        internal CBCharacteristic FindCharacteristic(string serviceUUID, string characteristicUUID)
        {
            if (string.IsNullOrEmpty(serviceUUID))
            {
                return services.SelectMany(s => s.characteristics).FirstOrDefault(c => c.uuid == characteristicUUID);
            } else {
                var service = services.FirstOrDefault(s => s.uuid == serviceUUID);
                if (service == null) {
                    UnityEngine.Debug.LogError($"CBPeripheral: service {serviceUUID} not found");
                    return null;
                }

                return service.characteristics.FirstOrDefault(c => c.uuid == characteristicUUID);
            }
        }

        public void DiscoverServices(string[] serviceUUIDs) => nativeMethods.DiscoverServices(this, serviceUUIDs);

        internal void OnDidDiscoverServices(CBService[] services, CBError error)
        {
            _services.Clear();
            _services.AddRange(services);
            peripheralDelegate?.DidDiscoverServices(this, error);
        }

        public void DiscoverCharacteristics(string[] characteristicUUIDs, CBService service) => nativeMethods.DiscoverCharacteristics(this, service, characteristicUUIDs);

        internal void OnDidDiscoverCharacteristics(CBCharacteristic[] characteristics, CBService service, CBError error)
        {
            service.UpdateCharacteristics(characteristics);
            peripheralDelegate?.DidDiscoverCharacteristics(this, service, error);
        }

        public void ReadValue(CBCharacteristic characteristic) => nativeMethods.ReadValueForCharacteristic(this, characteristic);

        internal void OnDidUpdateValueForCharacteristic(CBCharacteristic characteristic, CBError error)
        {
            peripheralDelegate?.DidUpdateValue(this, characteristic, error);
        }

        public void WriteValue(byte[] data, CBCharacteristic characteristic, CBCharacteristicWriteType type) => nativeMethods.WriteValueForCharacteristic(this, characteristic, data, type);

        internal void OnDidWriteValueForCharacteristic(CBCharacteristic characteristic, CBError error)
        {
            peripheralDelegate?.DidWriteValue(this, characteristic, error);
        }

        public void SetNotifyValue(bool enabled, CBCharacteristic characteristic) => nativeMethods.SetNotifyValueForCharacteristic(this, characteristic, enabled);

        internal void OnDidUpdateNotificationStateForCharacteristic(CBCharacteristic characteristic, bool isNotifying, CBError error)
        {
            characteristic.SetIsNotifying(isNotifying);
            peripheralDelegate?.DidUpdateNotificationState(this, characteristic, error);
        }

        public void ReadRSSI() => nativeMethods.ReadRSSI(this);
        internal void OnDidUpdateRSSI(CBError error) => peripheralDelegate?.DidUpdateRSSI(this, error);
        internal void OnDidReadRSSI(int rssi, CBError error) => peripheralDelegate?.DidReadRSSI(this, rssi, error);
    }

    // https://developer.apple.com/documentation/corebluetooth/cbperipheralstate
    public enum CBPeripheralState
    {
        disconnected = 0,
        connecting,
        connected,
        disconnecting
    }
}
