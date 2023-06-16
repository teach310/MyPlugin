using System.Collections;
using System.Collections.Generic;
using Sample;
using UnityEngine;
using UnityEngine.UI;

public class SampleScene : MonoBehaviour
{
    [SerializeField] Text label;

    public void OnClick()
    {
        label.text = "ここに歩数が入る";
        // Debug.Log("SamplePlugin.HelloWorld() = " + SamplePlugin.HelloWorld());
    }
}
