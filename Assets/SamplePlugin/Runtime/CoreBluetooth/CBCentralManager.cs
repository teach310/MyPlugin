using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbcentralmanager
    public class CBCentralManager : SafeHandle
    {
        static readonly Dictionary<IntPtr, CBCentralManager> instanceMap = new Dictionary<IntPtr, CBCentralManager>();
        // key: peripheralId, value: CBPeripheral
        Dictionary<string, CBPeripheral> peripherals = new Dictionary<string, CBPeripheral>();

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
            CentralEventHandler.Register(handle);
            return instance;
        }

        void OnDidUpdateState(CBManagerState state)
        {
            this.state = state;
            centralManagerDelegate?.CentralManagerDidUpdateState(this);
        }

        void OnDidDiscoverPeripheral(IntPtr peripheralIdPtr, IntPtr peripheralNamePtr)
        {
            var peripheral = new CBPeripheral(
                Marshal.PtrToStringUTF8(peripheralIdPtr),
                Marshal.PtrToStringUTF8(peripheralNamePtr)
            );

            peripherals[peripheral.identifier] = peripheral;
            centralManagerDelegate?.CentralManagerDidDiscoverPeripheral(this, peripheral);
        }

        static class CentralEventHandler
        {
            internal static void Register(IntPtr centralPtr)
            {
                NativeMethods.cb4u_central_manager_register_handlers(
                    centralPtr,
                    OnDidUpdateState,
                    OnDidDiscoverPeripheral
                );
            }

            static void CallInstanceMethod(IntPtr centralPtr, Action<CBCentralManager> action)
            {
                if (!instanceMap.TryGetValue(centralPtr, out var instance))
                {
                    UnityEngine.Debug.LogError("CBCentralManager instance not found.");
                    return;
                }

                action(instance);
            }

            [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UCentralManagerDidUpdateStateHandler))]
            internal static void OnDidUpdateState(IntPtr centralPtr, CBManagerState state)
            {
                CallInstanceMethod(centralPtr, instance => instance.OnDidUpdateState(state));
            }

            [AOT.MonoPInvokeCallback(typeof(NativeMethods.CB4UCentralManagerDidDiscoverPeripheralHandler))]
            internal static void OnDidDiscoverPeripheral(IntPtr centralPtr, IntPtr peripheralIdPtr, IntPtr peripheralNamePtr)
            {
                CallInstanceMethod(centralPtr, instance => instance.OnDidDiscoverPeripheral(peripheralIdPtr, peripheralNamePtr));
            }
        }
    }
}
