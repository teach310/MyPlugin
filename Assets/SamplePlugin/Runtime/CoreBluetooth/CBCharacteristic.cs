namespace CoreBluetooth
{
    // https://developer.apple.com/documentation/corebluetooth/cbcharacteristic
    public class CBCharacteristic
    {
        public string identifier { get; }

        /// <summary>
        /// The service to which this characteristic belongs.
        /// </summary>
        public CBService service { get; }

        // TODO: CBCharacteristicProperties properties { get; } キャッシュはしない
        public byte[] value { get; private set; }
        internal void SetValue(byte[] value) => this.value = value;

        public CBCharacteristic(string identifier, CBService service)
        {
            this.identifier = identifier;
            this.service = service;
        }

        public override string ToString()
        {
            return $"CBCharacteristic: identifier={identifier}";
        }
    }
}