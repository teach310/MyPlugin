using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CoreBluetooth
{
    public class CBService
    {
        public string uuid { get; }

        /// <summary>
        /// The peripheral to which this service belongs.
        /// </summary>
        public CBPeripheral peripheral { get; }

        List<CBCharacteristic> _characteristics = new List<CBCharacteristic>();
        public ReadOnlyCollection<CBCharacteristic> characteristics { get; }

        public CBService(string uuid, CBPeripheral peripheral)
        {
            this.uuid = uuid;
            this.peripheral = peripheral;
            characteristics = _characteristics.AsReadOnly();
        }

        public override string ToString()
        {
            return $"CBService: uuid={uuid}";
        }

        internal void UpdateCharacteristics(CBCharacteristic[] characteristics)
        {
            _characteristics.Clear();
            _characteristics.AddRange(characteristics);
        }
    }
}