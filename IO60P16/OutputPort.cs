namespace Gadgeteer.Modules.IanLee.IO60P16
{
    public class OutputPort : Port
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentModule">The module to which this pin belongs.</param>
        /// <param name="pin">The pin ID that this object represents.</param>
        /// <param name="initialState">The initial state of the port.</param>
        public OutputPort (IO60P16Module parentModule, IOPin pin, bool initialState) : base(parentModule, pin)
        {
            ParentModule.SetDirection((IOPin)PinNumber, PinDirection.Output);
            Write(initialState);
        }

        /// <summary>
        /// Set the state value of the pin.
        /// </summary>
        /// <param name="state">High (true) or low (false).</param>
        public void Write(bool state)
        {
            ParentModule.Write(PortNumber, PinNumber, state);
        }
    }
}
