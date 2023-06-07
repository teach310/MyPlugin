using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Sample
{
#if UNITY_EDITOR_OSX
    internal static partial class NativeMethods
    {
        const string SAMPLE_PLUGIN = "SamplePluginBundle";

        [DllImport(SAMPLE_PLUGIN, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sample_plugin_helloworld();
    }
#else
    internal static partial class NativeMethods
    {
        internal static int sample_plugin_helloworld() => 0;
    }
#endif
}
