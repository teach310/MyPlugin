using System.Text;

namespace CoreBluetooth
{
    internal class NativeCentralManagerProxy
    {
        readonly SafeCB4UCentralManagerHandle nativeCentralManagerHandle;

        internal NativeCentralManagerProxy(SafeCB4UCentralManagerHandle nativeCentralManagerHandle)
        {
            this.nativeCentralManagerHandle = nativeCentralManagerHandle;
        }

        internal string[] RetrievePeripheralsWithIdentifiers(string[] identifiers)
        {
            var sbSize = identifiers.Length * (36 + 1) + 1; // 36 is the length of UUID string, 1 is for comma.
            var sb = new StringBuilder(sbSize);
            var result = NativeMethods.cb4u_central_manager_retrieve_peripherals_with_identifiers(
                nativeCentralManagerHandle,
                identifiers,
                identifiers.Length,
                sb,
                sbSize
            );

            if (result < 0)
            {
                return new string[0];
            }

            return sb.ToString().Split(',');
        }

        internal void ScanForPeripherals(string[] serviceUUIDs)
        {
            NativeMethods.cb4u_central_manager_scan_for_peripherals(
                nativeCentralManagerHandle,
                serviceUUIDs,
                serviceUUIDs.Length
            );
        }

        internal void StopScan()
        {
            NativeMethods.cb4u_central_manager_stop_scan(nativeCentralManagerHandle);
        }

        internal bool IsScanning()
        {
            return NativeMethods.cb4u_central_manager_is_scanning(nativeCentralManagerHandle);
        }

        internal void Connect(string peripheralId)
        {
            var result = NativeMethods.cb4u_central_manager_connect_peripheral(
                nativeCentralManagerHandle,
                peripheralId
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute connect peripheral.");
            }
        }

        internal void CancelPeripheralConnection(string peripheralId)
        {
            var result = NativeMethods.cb4u_central_manager_cancel_peripheral_connection(
                nativeCentralManagerHandle,
                peripheralId
            );

            if (result < 0)
            {
                UnityEngine.Debug.LogError("Failed to execute cancel peripheral connection.");
            }
        }
    }
}
