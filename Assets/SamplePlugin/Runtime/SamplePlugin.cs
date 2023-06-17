using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Sample
{
    public class SamplePlugin
    {
        public static int HelloWorld()
        {
            return NativeMethods.sample_plugin_helloworld();
        }
#if UNITY_IOS && !UNITY_EDITOR
        static Action<bool> authorizeCompletion = null;

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.SamplePluginAuthorizeCompletion))]
        static void OnAuthorizeCompleted(bool requestSuccess)
        {
            authorizeCompletion?.Invoke(requestSuccess);
            authorizeCompletion = null;
        }

        public static void Authorize(Action<bool> completion)
        {
            authorizeCompletion = completion;
            NativeMethods.sample_plugin_authorize(OnAuthorizeCompleted);
        }

        static Action<int> getStepsTodayCompletion = null;

        [AOT.MonoPInvokeCallback(typeof(NativeMethods.SamplePluginGetStepsTodayCompletion))]
        static void OnGetStepsTodayCompleted(int steps)
        {
            getStepsTodayCompletion?.Invoke(steps);
            getStepsTodayCompletion = null;
        }

        public static void GetStepsToday(Action<int> completion)
        {
            getStepsTodayCompletion = completion;
            NativeMethods.sample_plugin_get_steps_today(OnGetStepsTodayCompleted);
        }
#else
        public static void Authorize(Action<bool> completion)
        {
            completion?.Invoke(true);
        }

        public static void GetStepsToday(Action<int> completion)
        {
            completion?.Invoke(0);
        }
#endif
    }
}
