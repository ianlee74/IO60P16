using System;
using Gadgeteer.Interfaces;

namespace Gadgeteer.Modules.IanLee.IO60P16
{
    public class InterruptPort : InputPort
    {
        public delegate void InterruptHandler(IOPin pin, bool pinState, DateTime timestamp);

        /// <summary>
        /// Event that is raised when an interrupt is triggered.
        /// </summary>
        public event InterruptHandler OnInterrupt;

        public InterruptPort(IO60P16Module parentModule, IOPin pin, ResistorMode resistorMode, InterruptMode interruptMode) : base(parentModule, pin)
        {
            Resistor = resistorMode;
            Interrupt = interruptMode;
            EnableInterrupt();
        }

        public InterruptMode Interrupt { get; set; }

        /// <summary>
        /// Enables interrupts for this port.
        /// </summary>
        public void EnableInterrupt()
        {
            ParentModule.SetInterruptEnable((IOPin)Id, true);
            ParentModule.Interrupt += OnParentInterrupt;
        }

        /// <summary>
        /// Handles interrupts raised by the module and throws a new event if the interrupt was for this pin.
        /// </summary>
        private void OnParentInterrupt(object sender, InterruptEventArgs args)
        {
            if ((byte)args.PinId != Id) return;
            if (OnInterrupt == null) return;
            if (Interrupt == InterruptMode.RisingEdge && !args.PinState) return;
            if (Interrupt == InterruptMode.FallingEdge && args.PinState) return;
            OnInterrupt(args.PinId, args.PinState, args.Timestamp);
        }

        /// <summary>
        /// Disable interrupts for this port.
        /// </summary>
        public void DisableInterrupt()
        {
            ParentModule.SetInterruptEnable((IOPin)Id, false);
            ParentModule.Interrupt -= OnParentInterrupt;
        }

        /// <summary>
        /// Clears the current interrupt on the interrupt port.
        /// Warning: This clears the interrupt of all pins on the same IO60P16 port.
        /// </summary>
        public void ClearInterrupt()
        {
            ParentModule.ReadRegister((byte)(IO60P16Module.INTERRUPT_STATUS_PORT_0_REGISTER + PortNumber));   
        }

        /// <summary>
        /// Dispose of the port.
        /// </summary>
        public override void Dispose()
        {
            DisableInterrupt();         // Disable interrupts for this pin on the module.
        }
    }
}
