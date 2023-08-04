using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Sample
{
#if UNITY_EDITOR_OSX || UNITY_IOS
    internal static partial class NativeMethods
    {
#if UNITY_IOS && !UNITY_EDITOR
        const string SAMPLE_PLUGIN = "__Internal";
#else
        const string SAMPLE_PLUGIN = "SamplePluginBundle";
#endif

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
