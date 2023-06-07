using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    public class SamplePlugin
    {
        public static int HelloWorld()
        {
            return NativeMethods.sample_plugin_helloworld();
        }
    }
}
