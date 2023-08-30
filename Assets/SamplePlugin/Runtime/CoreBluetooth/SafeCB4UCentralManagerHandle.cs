using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CoreBluetooth
{
    internal class SafeCB4UCentralManagerHandle : SafeHandle
    {
        static Dictionary<IntPtr, CBCentralManager> centralManagerMap = new Dictionary<IntPtr, CBCentralManager>();
        SafeCB4UCentralManagerHandle(IntPtr handle) : base(handle, true) { }

        public override bool IsInvalid => handle == IntPtr.Zero;

        internal static SafeCB4UCentralManagerHandle Create(CBCentralManager centralManager)
        {
            var handle = NativeMethods.cb4u_central_manager_new();
            var instance = new SafeCB4UCentralManagerHandle(handle);
            centralManagerMap.Add(handle, centralManager);
            return instance;
        }

        internal static bool TryGetCBCentralManager(IntPtr handle, out CBCentralManager centralManager)
        {
            return centralManagerMap.TryGetValue(handle, out centralManager);
        }

        protected override bool ReleaseHandle()
        {
            centralManagerMap.Remove(handle);
            NativeMethods.cb4u_central_manager_release(handle);
            return true;
        }
    }
}
