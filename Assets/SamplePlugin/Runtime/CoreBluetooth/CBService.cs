using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CoreBluetooth
{
    public class CBService
    {
        public string identifier { get; }

        /// <summary>
        /// The peripheral to which this service belongs.
        /// </summary>
        public CBPeripheral peripheral { get; }

        List<CBCharacteristic> _characteristics = new List<CBCharacteristic>();
        public ReadOnlyCollection<CBCharacteristic> characteristics { get; }

        public CBService(string identifier, CBPeripheral peripheral)
        {
            this.identifier = identifier;
            this.peripheral = peripheral;
            characteristics = _characteristics.AsReadOnly();
        }

        public override string ToString()
        {
            return $"CBService: identifier={identifier}";
        }

        internal void UpdateCharacteristics(CBCharacteristic[] characteristics)
        {
            _characteristics.Clear();
            _characteristics.AddRange(characteristics);
        }
    }
}