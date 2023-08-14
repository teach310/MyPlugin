using System.Collections;
using System.Collections.Generic;
using Sample;
using UnityEngine;
using CoreBluetooth;
using System;

public class SampleScene : MonoBehaviour
{
    public class BLESample : CBCentralManagerDelegate, IDisposable
    {
        string serviceUUID = "068c47b7-fc04-4d47-975a-7952be1a576f";

        CBCentralManager centralManager;

        public void Init()
        {
            centralManager = CBCentralManager.Create(this);
        }

        public void Scan()
        {
            if(centralManager.state != CBManagerState.poweredOn)
            {
                Debug.Log("Bluetooth is not powered on.");
                return;
            }

            if(centralManager.isScanning)
            {
                Debug.Log("Already scanning.");
                return;
            }

            centralManager.ScanForPeripherals(new string[]{serviceUUID});
        }

        public void CentralManagerDidUpdateState(CBCentralManager central)
        {
            Debug.Log($"CentralManagerDidUpdateState: {central.state}");
        }

        public void CentralManagerDidDiscoverPeripheral(CBCentralManager central, CBPeripheral peripheral)
        {
            Debug.Log($"CentralManagerDidDiscoverPeripheral: {peripheral}");
            central.StopScan();
        }

        public void Dispose()
        {
            centralManager.Dispose();
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
        // Debug.Log("SamplePlugin.HelloWorld() = " + SamplePlugin.HelloWorld());
        ble.Scan();
    }

    void OnDestroy()
    {
        ble.Dispose();
    }
}
