using System.Collections;
using System.Collections.Generic;
using Sample;
using UnityEngine;
using CoreBluetooth;
using System;
using System.Text;

public class SampleScene : MonoBehaviour
{
    public class BLESample : CBCentralManagerDelegate, CBPeripheralDelegate, IDisposable
    {
        string serviceUUID = "068c47b7-fc04-4d47-975a-7952be1a576f";
        string characteristicUUID = "e3737b3f-a08d-405b-b32d-35a8f6c64c5d";

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
            peripheral.peripheralDelegate = this;
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

            peripheral.DiscoverServices(new string[] { serviceUUID });
        }

        public void CentralManagerDidFailToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, CBError error)
        {
            Debug.Log($"CentralManagerDidFailToConnectPeripheral: {peripheral}");
        }

        public void CentralManagerDidDisconnectPeripheral(CBCentralManager central, CBPeripheral peripheral, CBError error)
        {
            Debug.Log($"CentralManagerDidDisconnectPeripheral: {peripheral}");
        }

        public void DidDiscoverServices(CBPeripheral peripheral, CBError error)
        {
            Debug.Log($"DidDiscoverServices: {peripheral}");
            foreach (var service in peripheral.services)
            {
                Debug.Log($"Service: {service}");
                peripheral.DiscoverCharacteristics(new string[] { characteristicUUID }, service);
            }
        }

        public void DidDiscoverCharacteristics(CBPeripheral peripheral, CBService service, CBError error)
        {
            Debug.Log($"DidDiscoverCharacteristics: {peripheral}");
            foreach (var characteristic in service.characteristics)
            {
                Debug.Log($"Characteristic: {characteristic}");
                if (characteristic.properties.HasFlag(CBCharacteristicProperties.Read))
                {
                    peripheral.ReadValue(characteristic);
                }
            }
        }

        public void DidUpdateValue(CBPeripheral peripheral, CBCharacteristic characteristic, CBError error)
        {
            Debug.Log($"DidUpdateValue: {peripheral}");
            if (error != null)
            {
                Debug.Log($"Error: {error}");
                return;
            }

            var str = Encoding.UTF8.GetString(characteristic.value);
            Debug.Log($"Data: {str}");
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
