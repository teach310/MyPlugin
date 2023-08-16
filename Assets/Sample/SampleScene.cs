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
        CBPeripheral peripheral;

        public void Init()
        {
            centralManager = CBCentralManager.Create(this);
        }

        public void Scan()
        {
            if (centralManager.state != CBManagerState.poweredOn)
            {
                Debug.Log("Bluetooth is not powered on.");
                return;
            }

            if (centralManager.isScanning)
            {
                Debug.Log("Already scanning.");
                return;
            }
            Debug.Log("Start scanning.");
            centralManager.ScanForPeripherals(new string[] { serviceUUID });
        }

        public bool IsConnected()
        {
            return peripheral != null && peripheral.state == CBPeripheralState.connected;
        }

        public void CentralManagerDidUpdateState(CBCentralManager central)
        {
            Debug.Log($"CentralManagerDidUpdateState: {central.state}");
        }

        public void CentralManagerDidDiscoverPeripheral(CBCentralManager central, CBPeripheral peripheral)
        {
            Debug.Log($"CentralManagerDidDiscoverPeripheral: {peripheral}");
            this.peripheral = peripheral;
            central.StopScan();
            central.Connect(peripheral);
        }

        public void Dispose()
        {
            centralManager.Dispose();
        }

        public void CentralManagerDidConnectPeripheral(CBCentralManager central, CBPeripheral peripheral)
        {
            Debug.Log($"CentralManagerDidConnectPeripheral: {peripheral}");
        }

        public void CentralManagerDidFailToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, CBError error)
        {
            Debug.Log($"CentralManagerDidFailToConnectPeripheral: {peripheral}");
        }

        public void CentralManagerDidDisconnectPeripheral(CBCentralManager central, CBPeripheral peripheral, CBError error)
        {
            Debug.Log($"CentralManagerDidDisconnectPeripheral: {peripheral}");
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
        if (ble.IsConnected())
        {
            Debug.Log("Already connected.");
        }
        else
        {
            ble.Scan();
        }
    }

    void OnDestroy()
    {
        ble.Dispose();
    }
}
