using System.Collections;
using System.Collections.Generic;
using Sample;
using UnityEngine;
using CoreBluetooth;
using System;
using System.Text;
using System.Linq;

public class SampleScene : MonoBehaviour
{
    public class BLESample : CBCentralManagerDelegate, CBPeripheralDelegate, IDisposable
    {
        string serviceUUID = "068C47B7-FC04-4D47-975A-7952BE1A576F";
        string characteristicUUID = "E3737B3F-A08D-405B-B32D-35A8F6C64C5D";
        string notifyCharacteristicUUID = "C9DA2CE8-D119-40D5-90F7-EF24627E8193";

        CBCentralManager centralManager;
        CBPeripheral peripheral;
        CBCharacteristic remoteCharacteristic;
        CBCharacteristic remoteNotifyCharacteristic;

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

        public void WriteData()
        {
            if (peripheral == null)
            {
                Debug.Log("Peripheral is null.");
                return;
            }

            if (remoteCharacteristic == null)
            {
                Debug.Log("Remote characteristic is null.");
                return;
            }

            var value = UnityEngine.Random.Range(100, 1000).ToString();
            var data = Encoding.UTF8.GetBytes(value);
            peripheral.WriteValue(data, remoteCharacteristic, CBCharacteristicWriteType.withResponse);
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
                peripheral.DiscoverCharacteristics(new string[] { characteristicUUID, notifyCharacteristicUUID }, service);
            }
        }

        public void DidDiscoverCharacteristics(CBPeripheral peripheral, CBService service, CBError error)
        {
            Debug.Log($"DidDiscoverCharacteristics: {peripheral}");
            var characteristics = service.characteristics;

            remoteCharacteristic = characteristics.FirstOrDefault(c => c.uuid == characteristicUUID);
            if (remoteCharacteristic == null)
            {
                Debug.Log($"Characteristic not found: {characteristicUUID}");
                return;
            }

            if (remoteCharacteristic.properties.HasFlag(CBCharacteristicProperties.Notify))
            {
                peripheral.SetNotifyValue(true, remoteCharacteristic);
            }

            remoteNotifyCharacteristic = characteristics.FirstOrDefault(c => c.uuid == notifyCharacteristicUUID);
            if (remoteNotifyCharacteristic != null)
            {
                peripheral.SetNotifyValue(true, remoteNotifyCharacteristic);
            }

            foreach (var characteristic in characteristics)
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
            Debug.Log($"DidUpdateValue: {characteristic}");
            if (error != null)
            {
                Debug.Log($"Error: {error}");
                return;
            }

            var str = Encoding.UTF8.GetString(characteristic.value);
            Debug.Log($"Data: {str}");
        }

        public void DidWriteValue(CBPeripheral peripheral, CBCharacteristic characteristic, CBError error)
        {
            Debug.Log($"DidWriteValue: {characteristic}");
            if (error != null)
            {
                Debug.Log($"Error: {error}");
                return;
            }
        }

        public void DidUpdateNotificationState(CBPeripheral peripheral, CBCharacteristic characteristic, CBError error)
        {
            Debug.Log($"DidUpdateNotificationState: {characteristic}");
            if (error != null)
            {
                Debug.Log($"Error: {error}");
                return;
            }
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
            ble.WriteData();
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
