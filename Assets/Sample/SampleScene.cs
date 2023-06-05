using System.Collections;
using System.Collections.Generic;
using Sample;
using UnityEngine;

public class SampleScene : MonoBehaviour
{
    public void OnClick()
    {
        Debug.Log("HelloPlugin.HelloWorld() = " + HelloPlugin.HelloWorld());
    }
}
