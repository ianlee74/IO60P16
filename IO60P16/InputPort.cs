namespace Gadgeteer.Modules.IanLee.IO60P16
{
    /// <summary>
    /// Represents an instance of an input port that can be used to read the value of a GPIO pin.
    /// </summary>
    public class InputPort : Port
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentModule">The instance of the parent IO60P16 module to which this pin is contained.</param>
        /// <param name="pin">The pin being setup for input.</param>
        public InputPort(IO60P16Module parentModule, IOPin pin) : base(parentModule, pin)
        {
            ParentModule.SetDirection(pin, PinDirection.Input);
        }
    }
}
