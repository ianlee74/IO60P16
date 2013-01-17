using System;

namespace Gadgeteer.Modules.IanLee.IO60P16
{
    public delegate void InterruptHandler(IOPin pin, bool pinState, DateTime timestamp);

    public abstract class Port : IDisposable
    {
        /// <summary>
        /// Event that is raised when an interrupt is triggered.
        /// </summary>
        public event InterruptHandler OnInterrupt;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentModule">The module to which this port belongs.</param>
        /// <param name="pin">The pin ID that this port represents.</param>
        protected Port(IO60P16Module parentModule, IOPin pin)
        {
            _parentModule = parentModule;
            _id = (byte)pin;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentModule">The module to which this port belongs.</param>
        /// <param name="pin">The pin ID that this port represents.</param>
        /// <param name="resistorMode">The resistor mode to assign to the pin.</param>
        protected Port(IO60P16Module parentModule, IOPin pin, ResistorMode resistorMode) : this(parentModule, pin)
        {
            Resistor = resistorMode;
        }

        /// <summary>
        /// The module to which this pin belongs.
        /// </summary>
        public IO60P16Module ParentModule { get { return _parentModule; } }
        private readonly IO60P16Module _parentModule;

        /// <summary>
        /// The pin ID that this port represents.
        /// </summary>
        public byte Id { get { return _id; } }
        private readonly byte _id;

        /// <summary>
        /// The [module silk screened] port number.
        /// </summary>
        public byte PortNumber
        {
            get { return (byte)(_id >> 4); }
        }

        /// <summary>
        /// The [module silk screened] pin number.
        /// </summary>
        public byte PinNumber
        {
            get { return (byte)(_id & 0xF); }
        }

        /// <summary>
        /// Enables interrupts for this port.
        /// </summary>
        public void EnableInterrupt()
        {
            ParentModule.SetInterruptEnable((IOPin) Id, true);
            ParentModule.Interrupt += OnParentInterrupt;
        }

        /// <summary>
        /// Handles interrupts raised by the module and throws a new event if the interrupt was for this pin.
        /// </summary>
        private void OnParentInterrupt(object sender, InterruptEventArgs args)
        {
            if ((byte)args.PinId != Id) return;
            if (OnInterrupt != null)
            {
                OnInterrupt(args.PinId, args.PinState, args.Timestamp);
            }
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
        /// Read the current value of the port.
        /// </summary>
        /// <returns>State value of the port.</returns>
        public virtual bool Read()
        {
            return ParentModule.Read((IOPin)Id);
        }

        /// <summary>
        /// Dispose of the port.
        /// </summary>
        public virtual void Dispose()
        {
            DisableInterrupt();         // Disable interrupts for this pin on the module.
        }

        /// <summary>
        /// Gets or sets the resistor mode of the input port. You set the initial resistor mode value in the constructor.
        /// </summary>
        public ResistorMode Resistor
        {
            get { return ParentModule.GetResistorMode((IOPin) Id); }
            set { ParentModule.SetResistorMode((IOPin)Id, value); }
        }
    }
}
