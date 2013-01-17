using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;

namespace Gadgeteer.Modules.IanLee.IO60P16
{
    public class PWM : IDisposable
    {

        public enum ScaleFactor : uint
        {
            Milliseconds = 1000000,
            Microseconds = 1000,
            Nanoseconds  = 1,               // Native resolution will be in nanoseconds.
        }

        public enum PwmClockSource : byte
        {
            /// <summary>
            /// 32 kHz clock source.  Has a maximum period of 7.97 ms.
            /// </summary>
            Clock_32kHz,
            /// <summary>
            /// 24 MHz clock.  Has a maximum period of 11 ns.
            /// </summary>
            Clock_24MHz,
            /// <summary>
            /// 1.5 MHz clock.  Has a maximum period of 170 ns.
            /// </summary>
            Clock_1Mhz5,
            /// <summary>
            /// 93.75 kHz clock.  Has a maximum period of 2.72 ms.
            /// </summary>
            Clock_93kHz75,
            /// <summary>
            /// 367.6 Hz clock.  Has a maximum period of 693 ms.  This is a user programmable clock.
            /// It is the 93.75 kHz clock used in combination with a divider value between 0-255.
            /// 255 is the default divider values (e.g. 93750/255 = 367.6)
            /// </summary>
            Clock_367Hz6,
            /// <summary>
            /// Use the clock that was previously set.
            /// </summary>
            PreviousPwm
        }

        public PWM(IO60P16Module parentModule, PwmPin pin, double frequency_Hz, double dutyCycle, bool invertOutput)
        {
            _parentModule = parentModule;
            _pin = pin;
            _frequency = frequency_Hz;
            _dutyCycle = dutyCycle;
            _invertOutput = invertOutput;
        }

        public PWM(IO60P16Module parentModule, PwmPin pin, uint period, uint duration, ScaleFactor scale, bool invertOutput)
        {
            _parentModule = parentModule;
            _pin = pin;
            _period_ns = period * (uint)scale;
            _duration_ns = duration * (uint)scale;
            _scale = scale;
            _invertOutput = invertOutput;
        }

        private IO60P16Module _parentModule;
        public IO60P16Module ParentModule
        {
            get { return _parentModule; }
            set { _parentModule = value; }
        }

        private readonly PwmPin _pin;
        public PwmPin Pin { get { return _pin; } }

        public bool InvertOutput
        {
            get { return _invertOutput; }
            set { _invertOutput = value; }
        }
        private bool _invertOutput;

        public double Frequency
        {
            get { return _frequency; }
            set 
            { 
                _frequency = value;
            }
        }
        private double _frequency;

        public double DutyCycle
        {
            get { return _dutyCycle; }
            set 
            { 
                _dutyCycle = value;
            }
        }
        private double _dutyCycle;

        /// <summary>
        /// The period length of the PWM signal.  Units are determined by Scale property.
        /// </summary>
        public uint Period
        {
            get { return _period_ns / (uint)_scale; }
            set 
            { 
                _period_ns = value * (uint)_scale;
            }
        }
        private uint _period_ns;

        /// <summary>
        /// The pulse width duration.  Units are determined by Scale property.
        /// </summary>
        public uint Duration
        {
            get { return _duration_ns / (uint)_scale; }
            set 
            {
                _duration_ns = value * (uint)_scale;
            }
        }
        private uint _duration_ns;

        /// <summary>
        /// The scale 
        /// </summary>
        public ScaleFactor Scale
        {
            get { return _scale; }
            set 
            { 
                _scale = value;
            }
        }
        private ScaleFactor _scale;

        public void Start()
        {
            SetPwm((byte)((byte)_pin >> 4), (byte)((byte)_pin & 0xF), _period_ns, _duration_ns );   
        }

        public void Stop()
        {
            
        }

        public static void Start(PWM[] ports)
        {
            
        }

        public void Stop(PWM[] ports)
        {

        }

        protected static PwmClockSource SelectClock(uint period_ns, out byte period, out byte divider)
        {
            // Tout = periodns = 1/clock * period * 1000
            // period = periodns / (1/clock) * 1000
            //        = clock / periodns * 1000

            PwmClockSource clock;
            divider = 0;

            if (period_ns < 11000)
            {
                clock = PwmClockSource.Clock_24MHz;
                period = (byte)(.024 * period_ns);
            }
            else if (period_ns < 170000)
            {
                clock = PwmClockSource.Clock_1Mhz5;
                period = (byte)(.0015 * period_ns);
            }
            else if (period_ns < 2720000)
            {
                clock = PwmClockSource.Clock_93kHz75;
                period = (byte)(.00009375 * period_ns);
            }
            else if (period_ns < 7970000)
            {
                clock = PwmClockSource.Clock_32kHz;
                period = (byte)(.000032 * period_ns);
            }
            else if (period_ns <= 693000000)
            {
                clock = PwmClockSource.Clock_367Hz6;
                // Calc the best divider.
                divider = (byte)Math.Round(period_ns / 2720085.0);        // Round() is necessary.  Otherwise, it always rounds down.
                period = 255;

                /* Why 2720.085?  Here's why...
                 * Tout_ms = divider / 93.75kHz * period
                 * Tout_ns = divider / 93.75kHz * period * 1000
                 * Tout_ns = divider * period * 10.667
                 * divider = Tout_ns / period / 10.667
                 * divider = Tout_ns / 255 / 10.667               <= default period to max (255)
                 * divider = Tout_ns / 2720.085 (uS)
                 */
            }
            else
            {
                throw new ArgumentOutOfRangeException("period_ns", "Period cannot be longer than 693 mS.");
            }

            return clock;
        }

        private void SetPwm(byte port, byte pin, byte period, byte pulseWidth, PWM.PwmClockSource clock, byte clockDivider = 0)
        {
            //_parentModule.Write(port, pin, false);                      // Why was this here???

            _parentModule.WriteRegister(0x18, port);                      // Select port

            var b = _parentModule.ReadRegister(0x1a)[0];
            b |= (byte)((1 << pin));
            _parentModule.WriteRegister(0x1a, b);                         // select PWM for port output

            b = _parentModule.ReadRegister(0x1C)[0];
            b &= (byte)(~(1 << pin));
            _parentModule.WriteRegister(0x1C, b);                         // Set pin for output.

            _parentModule.WriteRegister(0x28, (byte)(0x08 + pin));        // Select the PWM pin to configure.

            _parentModule.WriteRegister(0x29, (byte)clock);               // Config PWM (select 32kHz clock source)
            if (clockDivider > 0) _parentModule.WriteRegister(0x2c, clockDivider);     // Set the clock divider (if using 367.6 Hz clock)
            _parentModule.WriteRegister(0x2a, period);                    // set the period (0-256)
            _parentModule.WriteRegister(0x2b, pulseWidth);                // set the pulse width (0-(period-1))

            //_parentModule.Write(port, pin, true);
        }

        public void SetPwm(byte port, byte pin, uint period_ns, uint pulseWidth_ns)
        {
            byte period;
            byte clockDivider;
            var clock = SelectClock(period_ns, out period, out clockDivider);
            var dutyCycle = (float)pulseWidth_ns / (float)period_ns;
            var pulseWidth = dutyCycle * period;
            SetPwm(port, pin, period, (byte)pulseWidth, clock, clockDivider);
        }

        #region IDisposable Members

        public void Dispose()
        {

        }

        ~PWM()
        {
            
        }

        #endregion
    }
}
