using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbcentralmanager
    public class CBCentralManager : SafeHandle
    {
        static readonly Dictionary<IntPtr, CBCentralManager> instanceMap = new Dictionary<IntPtr, CBCentralManager>();

        public override bool IsInvalid => handle == IntPtr.Zero;

        CBCentralManager(IntPtr handle) : base(IntPtr.Zero, true)
        {
            this.handle = handle;
            instanceMap.Add(handle, this);
        }

        protected override bool ReleaseHandle()
        {
            instanceMap.Remove(handle);
            NativeMethods.cb4u_central_manager_release(handle);
            return true;
        }

        public CBManagerState state { get; private set; } = CBManagerState.unknown;
        public CBCentralManagerDelegate centralManagerDelegate { get; set; }

        public void ScanForPeripherals(string[] serviceUUIDs)
        {
            NativeMethods.cb4u_central_manager_scan_for_peripherals(
                handle,
                serviceUUIDs,
                serviceUUIDs.Length
            );
        }

        public void StopScan() => NativeMethods.cb4u_central_manager_stop_scan(handle);
        public bool isScanning => NativeMethods.cb4u_central_manager_is_scanning(handle);

        // NOTE: In the original CBCentralManager, queue and options are optional arguments,
        //       but in this class, they will be implemented if necessary.
        public static CBCentralManager Create(CBCentralManagerDelegate centralManagerDelegate = null)
        {
            var handle = NativeMethods.cb4u_central_manager_new();
            var instance = new CBCentralManager(handle);
            instance.centralManagerDelegate = centralManagerDelegate;
            NativeMethods.cb4u_central_manager_register_handlers(
                handle,
                OnDidUpdateState,
                OnDidDiscoverPeripheral
            );
            return instance;
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UCentralManagerDidUpdateStateHandler))]
        static void OnDidUpdateState(IntPtr centralPtr, CBManagerState state)
        {
            if (!instanceMap.TryGetValue(centralPtr, out var instance))
            {
                UnityEngine.Debug.LogError("CBCentralManager instance not found.");
                return;
            }

            instance.state = state;
            instance.centralManagerDelegate?.CentralManagerDidUpdateState(instance);
        }

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UCentralManagerDidDiscoverPeripheralHandler))]
        static void OnDidDiscoverPeripheral(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr peripheralNamePtr)
        {
            if (!instanceMap.TryGetValue(centralPtr, out var instance))
            {
                UnityEngine.Debug.LogError("CBCentralManager instance not found.");
                return;
            }

            var peripheral = new CBPeripheral(
                Marshal.PtrToStringUTF8(peripheralIdPtr),
                Marshal.PtrToStringUTF8(peripheralNamePtr)
            );
            instance.centralManagerDelegate?.CentralManagerDidDiscoverPeripheral(instance, peripheral);
        }
    }
}
