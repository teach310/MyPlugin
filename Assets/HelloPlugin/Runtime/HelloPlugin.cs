using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    public class HelloPlugin
    {
        public static int HelloWorld()
        {
            return NativeMethods.hello_plugin_helloworld();
        }
    }
}
