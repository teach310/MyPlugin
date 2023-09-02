namespace CoreBluetooth
{
    internal class NativeCharacteristicProxy : ICharacteristicNativeMethods
    {
        readonly SafeCB4UCentralManagerHandle nativeCentralManagerHandle;

        public NativeCharacteristicProxy(SafeCB4UCentralManagerHandle nativeCentralManagerHandle)
        {
            this.nativeCentralManagerHandle = nativeCentralManagerHandle;
        }

        CBCharacteristicProperties ICharacteristicNativeMethods.GetCharacteristicProperties(CBCharacteristic characteristic)
        {
            var propertiesInt = NativeMethods.cb4u_central_manager_characteristic_properties(
                nativeCentralManagerHandle,
                characteristic.service.peripheral.identifier,
                characteristic.service.uuid,
                characteristic.uuid
            );

            if (propertiesInt < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute get characteristic properties.");
            }
            return (CBCharacteristicProperties)propertiesInt;
        }
    }
}