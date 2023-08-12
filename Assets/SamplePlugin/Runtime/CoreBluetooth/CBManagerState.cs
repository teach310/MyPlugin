namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbmanagerstate
    public enum CBManagerState
    {
        unknown = 0,
        resetting,
        unsupported,
        unauthorized,
        poweredOff,
        poweredOn
    }
}
