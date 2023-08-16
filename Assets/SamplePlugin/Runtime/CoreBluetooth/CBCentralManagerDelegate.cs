namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbcentralmanagerdelegate
    // Original CBCentralManagerDelegate has only centralManagerDidUpdateState as required method.
    // However, in this interface, all methods that are not used should be empty methods, so all methods are required.
    public interface CBCentralManagerDelegate
    {
        void CentralManagerDidUpdateState(CBCentralManager central);
        void CentralManagerDidDiscoverPeripheral(CBCentralManager central, CBPeripheral peripheral);
        void CentralManagerDidConnectPeripheral(CBCentralManager central, CBPeripheral peripheral);
        void CentralManagerDidFailToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, CBError error);
        void CentralManagerDidDisconnectPeripheral(CBCentralManager central, CBPeripheral peripheral, CBError error);
    }
}
