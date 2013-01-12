namespace Gadgeteer.Modules.GHIElectronics.IO60P16
{
    public class InterruptPort : InputPort
    {
        public InterruptPort(IO60P16Module parentModule, IOPin pin) : base(parentModule, pin)
        {
            EnableInterrupt();
        }

        /// <summary>
        /// Clears the current interrupt on the interrupt port.
        /// Warning: This clears the interrupt of all pins on the same IO60P16 port.
        /// </summary>
        public void ClearInterrupt()
        {
            ParentModule.ReadRegister((byte)(IO60P16Module.INTERRUPT_STATUS_PORT_0_REGISTER + PortNumber));   
        }    
    }
}
