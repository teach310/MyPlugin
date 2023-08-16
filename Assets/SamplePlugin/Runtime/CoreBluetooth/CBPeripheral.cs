namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbperipheral
    public class CBPeripheral
    {
        public string identifier { get; private set; }
        public string name { get; private set; }
        public CBPeripheralState state { get; private set; }

        public CBPeripheral(string id, string name, CBPeripheralState state = CBPeripheralState.disconnected)
        {
            this.identifier = id;
            this.name = name;
            this.state = state;
        }

        // TODO: output mtu
        public override string ToString()
        {
            return $"CBPeripheral: identifier = {identifier}, name = {name}, state = {state}";
        }

        internal void SetState(CBPeripheralState state)
        {
            this.state = state;
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
