using System.Text;

namespace CoreBluetooth
{
    internal class NativePeripheralProxy : IPeripheralNativeMethods
    {
        readonly SafeCB4UCentralManagerHandle nativeCentralManagerHandle;

        internal NativePeripheralProxy(SafeCB4UCentralManagerHandle nativeCentralManagerHandle)
        {
            this.nativeCentralManagerHandle = nativeCentralManagerHandle;
        }

        internal string Name(string peripheralId)
        {
            var sb = new StringBuilder(256);
            var result = NativeMethods.cb4u_central_manager_peripheral_name(
                nativeCentralManagerHandle,
                peripheralId,
                sb,
                sb.Capacity
            );

            if (result < 0)
            {
                return string.Empty;
            }

            return sb.ToString();
        }

        void IPeripheralNativeMethods.DiscoverServices(CBPeripheral peripheral, string[] serviceUUIDs)
        {
            int result = NativeMethods.cb4u_peripheral_discover_services(
                nativeCentralManagerHandle,
                peripheral.identifier,
                serviceUUIDs,
                serviceUUIDs.Length
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute discover services.");
            }
        }

        void IPeripheralNativeMethods.DiscoverCharacteristics(CBPeripheral peripheral, CBService service, string[] characteristicUUIDs)
        {
            int result = NativeMethods.cb4u_peripheral_discover_characteristics(
                nativeCentralManagerHandle,
                peripheral.identifier,
                service.uuid,
                characteristicUUIDs,
                characteristicUUIDs.Length
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute discover characteristics.");
            }
        }

        void IPeripheralNativeMethods.ReadValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic)
        {
            int result = NativeMethods.cb4u_peripheral_read_value_for_characteristic(
                nativeCentralManagerHandle,
                peripheral.identifier,
                characteristic.service.uuid,
                characteristic.uuid
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute read value for characteristic.");
            }
        }

        void IPeripheralNativeMethods.WriteValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic, byte[] data, CBCharacteristicWriteType type)
        {
            int result = NativeMethods.cb4u_peripheral_write_value_for_characteristic(
                nativeCentralManagerHandle,
                peripheral.identifier,
                characteristic.service.uuid,
                characteristic.uuid,
                data,
                data.Length,
                (int)type
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute write value for characteristic.");
            }
        }

        void IPeripheralNativeMethods.SetNotifyValueForCharacteristic(CBPeripheral peripheral, CBCharacteristic characteristic, bool enabled)
        {
            int result = NativeMethods.cb4u_peripheral_set_notify_value_for_characteristic(
                nativeCentralManagerHandle,
                peripheral.identifier,
                characteristic.service.uuid,
                characteristic.uuid,
                enabled
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute set notify value for characteristic.");
            }
        }

        void IPeripheralNativeMethods.ReadRSSI(CBPeripheral peripheral)
        {
            int result = NativeMethods.cb4u_central_manager_peripheral_read_rssi(
                nativeCentralManagerHandle,
                peripheral.identifier
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute read RSSI.");
            }
        }

        CBPeripheralState IPeripheralNativeMethods.GetPeripheralState(CBPeripheral peripheral)
        {
            var stateInt = NativeMethods.cb4u_central_manager_peripheral_state(nativeCentralManagerHandle, peripheral.identifier);

            if (stateInt < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute get peripheral state.");
            }
            return (CBPeripheralState)stateInt;
        }
    }
}
