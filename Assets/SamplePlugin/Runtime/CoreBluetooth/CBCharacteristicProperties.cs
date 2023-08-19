using System;

namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbcharacteristicproperties
    [Flags]
    public enum CBCharacteristicProperties
    {
        Broadcast = 1,
        Read = 2,
        WriteWithoutResponse = 4,
        Write = 8,
        Notify = 16,
        Indicate = 32,
        AuthenticatedSignedWrites = 64,
        ExtendedProperties = 128,
        NotifyEncryptionRequired = 256,
        IndicateEncryptionRequired = 512
    }
}
