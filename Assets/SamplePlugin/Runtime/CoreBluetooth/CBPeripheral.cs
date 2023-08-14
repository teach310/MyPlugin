namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbperipheral
    public class CBPeripheral
    {
        public string identifier { get; private set; }
        public string name { get; private set; }

        public CBPeripheral(string id, string name)
        {
            this.identifier = id;
            this.name = name;
        }

        // Xcode log 参考
        // Device found: <CBPeripheral: 0x283510000, identifier = 36958114-4913-C3C3-C16F-1CF3831B8211, name = M5AtomS3 BLE Server, mtu = 0, state = disconnected>
        public override string ToString()
        {
            return $"CBPeripheral: identifier = {identifier}, name = {name}";
        }
    }
}