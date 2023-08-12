using System.Collections;
using System.Collections.Generic;
using Sample;
using UnityEngine;
using CoreBluetooth;

public class SampleScene : MonoBehaviour
{
    public class BLESample : CBCentralManagerDelegate
    {
        CBCentralManager centralManager;

        public void Init()
        {
            centralManager = CBCentralManager.Create(this);
        }

        public void CentralManagerDidUpdateState(CBCentralManager central)
        {
            Debug.Log($"CentralManagerDidUpdateState: {central.state}");
        }
    }

    BLESample ble;

    void Start()
    {
        ble = new BLESample();
        ble.Init();
    }

    public void OnClick()
    {
        Debug.Log("SamplePlugin.HelloWorld() = " + SamplePlugin.HelloWorld());
    }
}
