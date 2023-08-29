namespace CoreBluetooth
{
    public interface CBPeripheralDelegate
    {
        void DidDiscoverServices(CBPeripheral peripheral, CBError error);
        void DidDiscoverCharacteristics(CBPeripheral peripheral, CBService service, CBError error);
        void DidUpdateValue(CBPeripheral peripheral, CBCharacteristic characteristic, CBError error);
        void DidWriteValue(CBPeripheral peripheral, CBCharacteristic characteristic, CBError error);
        void DidUpdateNotificationState(CBPeripheral peripheral, CBCharacteristic characteristic, CBError error);
        // void DidDiscoverDescriptors(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);
        void DidUpdateRSSI(CBPeripheral peripheral, CBError error); // for macOS
        void DidReadRSSI(CBPeripheral peripheral, int rssi, CBError error);  // for iOS
        // void DidDiscoverIncludedServices(CBPeripheral peripheral, CBService service, NSError error);
        // void DidModifyServices(CBPeripheral peripheral, NSArray services);
    }
}