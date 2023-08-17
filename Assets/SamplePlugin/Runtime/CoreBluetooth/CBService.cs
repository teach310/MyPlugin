namespace CoreBluetooth
{
    public class CBService
    {
        public string identifier { get; }

        /// <summary>
        /// The peripheral to which this service belongs.
        /// </summary>
        public CBPeripheral peripheral { get; }

        public CBService(string identifier, CBPeripheral peripheral)
        {
            this.identifier = identifier;
            this.peripheral = peripheral;
        }

        public override string ToString()
        {
            return $"CBService: identifier={identifier}";
        }
    }
}