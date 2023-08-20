using System;

namespace CoreBluetooth
{
    internal interface ICharacteristicNativeMethods
    {
        CBCharacteristicProperties GetCharacteristicProperties(CBCharacteristic characteristic);
    }

    // https://developer.apple.com/documentation/corebluetooth/cbcharacteristic
    public class CBCharacteristic
    {
        public string uuid { get; }

        /// <summary>
        /// The service to which this characteristic belongs.
        /// </summary>
        public CBService service { get; }
        ICharacteristicNativeMethods nativeMethods;

        public byte[] value { get; private set; }
        internal void SetValue(byte[] value) => this.value = value;

        internal CBCharacteristic(string uuid, CBService service, ICharacteristicNativeMethods nativeMethods)
        {
            this.uuid = uuid;
            this.service = service;
            this.nativeMethods = nativeMethods;
        }

        public override string ToString()
        {
            var valueText = value == null ? "null" : $"{{length = {value.Length}, bytes = {BitConverter.ToString(value).Replace("-", "")}}}";
            return $"CBCharacteristic: UUID={uuid}, properties={properties}, value={valueText}";
        }

        public CBCharacteristicProperties properties => nativeMethods.GetCharacteristicProperties(this);
    }
}
