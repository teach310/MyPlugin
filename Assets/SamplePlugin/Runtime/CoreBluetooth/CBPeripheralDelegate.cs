namespace CoreBluetooth
{
    public interface CBPeripheralDelegate
    {
        void DidDiscoverServices(CBPeripheral peripheral, CBError error);
        // void DidDiscoverCharacteristics(CBPeripheral peripheral, CBService service, NSError error);
        // void DidUpdateValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);
        // void DidWriteValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);
        // void DidUpdateNotificationState(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);
        // void DidDiscoverDescriptors(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);
        // void DidUpdateValue(CBPeripheral peripheral, CBDescriptor descriptor, NSError error);
        // void DidWriteValue(CBPeripheral peripheral, CBDescriptor descriptor, NSError error);
        // void DidUpdateRSSI(CBPeripheral peripheral, NSError error);
        // void DidReadRSSI(CBPeripheral peripheral, NSNumber rssi, NSError error);
        // void DidDiscoverIncludedServices(CBPeripheral peripheral, CBService service, NSError error);
        // void DidModifyServices(CBPeripheral peripheral, NSArray services);

    }
}