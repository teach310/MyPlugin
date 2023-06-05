using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Sample
{
#if UNITY_EDITOR_OSX
    internal static partial class NativeMethods
    {
        const string DLL_NAME = "HelloPlugin";

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int hello_plugin_helloworld();
    }
#else
    internal static partial class NativeMethods
    {
        internal static int hello_plugin_helloworld() => 0;
    }
#endif
}
