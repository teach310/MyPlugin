using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbperipheral
    public class CBPeripheral
    {
        CBCentralManager centralManager;
        public string identifier { get; }
        public string name { get; }
        public CBPeripheralState state { get; private set; }
        public CBPeripheralDelegate peripheralDelegate { get; set; }
        List<CBService> _services = new List<CBService>();
        public ReadOnlyCollection<CBService> services { get; }

        public CBPeripheral(CBCentralManager centralManager, string id, string name, CBPeripheralState state = CBPeripheralState.disconnected)
        {
            this.centralManager = centralManager;
            this.identifier = id;
            this.name = name;
            this.state = state;
            this.services = _services.AsReadOnly();
        }

        // TODO: output mtu
        public override string ToString()
        {
            return $"CBPeripheral: identifier = {identifier}, name = {name}, state = {state}";
        }

        internal void SetState(CBPeripheralState state) => this.state = state;
        public void DiscoverServices(string[] serviceUUIDs) => centralManager.DiscoverServices(this, serviceUUIDs);

        internal void OnDidDiscoverServices(CBService[] services, CBError error)
        {
            _services.Clear();
            _services.AddRange(services);
            peripheralDelegate?.DidDiscoverServices(this, error);
        }

        public void DiscoverCharacteristics(string[] characteristicUUIDs, CBService service) => centralManager.DiscoverCharacteristics(this, service, characteristicUUIDs);

        internal void OnDidDiscoverCharacteristics(CBCharacteristic[] characteristics, CBService service, CBError error)
        {
            service.UpdateCharacteristics(characteristics);
            peripheralDelegate?.DidDiscoverCharacteristics(this, service, error);
        }

        public void ReadValue(CBCharacteristic characteristic) => centralManager.ReadValueForCharacteristic(this, characteristic);

        internal void OnDidUpdateValueForCharacteristic(CBCharacteristic characteristic, CBError error)
        {
            peripheralDelegate?.DidUpdateValue(this, characteristic, error);
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
