using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CoreBluetooth
{
    internal interface IPeripheralNativeMethods
    {
        void DiscoverServices(CBPeripheral peripheral, string[] serviceUUIDs);
        void DiscoverCharacteristics(CBPeripheral peripheral, CBService service, string[] characteristicUUIDs);
        void ReadValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic);
        void WriteValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic, byte[] data, CBCharacteristicWriteType type);
    }

    // https://developer.apple.com/documentation/corebluetooth/cbperipheral
    public class CBPeripheral
    {
        IPeripheralNativeMethods nativeMethods;
        public string identifier { get; }
        public string name { get; }
        public CBPeripheralState state { get; private set; }
        public CBPeripheralDelegate peripheralDelegate { get; set; }
        List<CBService> _services = new List<CBService>();
        public ReadOnlyCollection<CBService> services { get; }

        internal CBPeripheral(string id, string name, IPeripheralNativeMethods nativeMethods, CBPeripheralState state = CBPeripheralState.disconnected)
        {
            this.identifier = id;
            this.name = name;
            this.nativeMethods = nativeMethods;
            this.state = state;
            this.services = _services.AsReadOnly();
        }

        // TODO: output mtu
        public override string ToString()
        {
            return $"CBPeripheral: identifier = {identifier}, name = {name}, state = {state}";
        }

        internal void SetState(CBPeripheralState state) => this.state = state;
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
