using System;
using System.Threading;
using Gadgeteer;
using Gadgeteer.Interfaces;
using Gadgeteer.Modules.IanLee.IO60P16;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
//using NETMFx.LedCube.Effects;
//using NETMFx.LedCube.IO60P16;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using OutputPort = Gadgeteer.Modules.IanLee.IO60P16.OutputPort;
using InputPort = Gadgeteer.Modules.IanLee.IO60P16.InputPort;
using InterruptPort = Gadgeteer.Modules.IanLee.IO60P16.InterruptPort;
using PWM = Gadgeteer.Modules.IanLee.IO60P16.PWM;
using ResistorMode = Gadgeteer.Modules.IanLee.IO60P16.ResistorMode;

namespace Test_App
{
    public partial class Program
    {
        private static GTM.IanLee.IO60P16.IO60P16Module io60p16;

        private static byte period = 0xfe;
        private static byte pulseWidth = 0x18;

        private static OutputPort op12;
        private static OutputPort op15;
        private static PWM pwm;
        private static PWM pwm2;
        private static InterruptPort ip0;

        //private static LedCube3 _ledCube;
        // Create a function to replace .Start()
        private void SetPwm(IO60P16Module _parentModule, byte port, byte pin, byte period, byte pulseWidth, PWM.PwmClockSource clock, byte clockDivider = 0)
        {
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
        }

        void BareMetalTest()
        {
            const int PORT = 6;
            const int PIN = 0x11;   // pin #1

            // 1.  Read current status of interrupt enable register.
            io60p16.WriteRegister(0x18, 7);                 // Select port
            byte intStatus = io60p16.ReadRegister(0x19)[0];
            Debug.Print("InterruptEnable:  " + intStatus);
            // 2.  Enable interrupt.
            io60p16.WriteRegister(0x18, PORT);                 // Select port
            io60p16.WriteRegister(0x19, PIN);               // Set interrupt enable bit.
            // 3.  Keep reading values...
            while(true)
            {
                io60p16.WriteRegister(0x18, PORT);                 // Select port
                intStatus = io60p16.ReadRegister(PIN)[0];
                Debug.Print("InterruptEnable:  " + intStatus);
                Thread.Sleep(500);
            };
        }

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            io60p16 = new GTM.IanLee.IO60P16.IO60P16Module(7);

            Debug.Print("Program Started");

            byte intStatus;
            ip0 = io60p16.CreateInterruptPort(IOPin.Port6_Pwm1, ResistorMode.ResistivePullUp, InterruptMode.FallingEdge);
            ip0.OnInterrupt += (pin, pinState, timestamp) =>
                {
                    Debug.Print("Bam! [" + pin + ", " + pinState + ", " + timestamp + "]");
                };
            var t2 = new GT.Timer(1000);
            t2.Tick += timer =>
                           {
                               io60p16.WriteRegister(0x18, 6); // Select port
                               intStatus = io60p16.ReadRegister(0x19)[0];
                               Debug.Print("InterruptEnable:  " + intStatus);
                           };
            t2.Start();
            return;
/*
            // Bare metal test.
            var t2 = new Thread(BareMetalTest);
            t2.Start();
            return;


            Debug.Print("InterruptEnable:  " + io60p16.GetInterruptsEnabled(7));
            op12 = io60p16.CreateOutputPort(IOPin.Port6_Pwm6, false);
            ip0 = io60p16.CreateInterruptPort(IOPin.Port7_Pwm14);
            ip0.OnInterrupt += (data1, data2, time) => Debug.Print("Exterminate!");
            Debug.Print("InterruptEnable:  " + io60p16.GetInterruptsEnabled(7));
            //var t = new Thread(Test_Thread);
            //t.Start();
            op12.Write(true);
            ip0.ClearInterrupt();

            var t = new GT.Timer(1000);
            t.Tick += tmr =>
            {
                //ip0.ClearInterrupt();
                Debug.Print("Tick.");
                op12.Write(true);
                Thread.Sleep(200);
                op12.Write(false);
            };
            //t.Start();
            return;

            // Test interrupts on an IO pin.
            io60p16.SetDirection(IOPin.Port0_Pin6, PinDirection.Output);
            io60p16.SetDirection(IOPin.Port7_Pwm15, PinDirection.Input);
            io60p16.SetInterruptEnable(IOPin.Port7_Pwm15, true);
            Debug.Print("InterruptEnable:  " + io60p16.GetInterruptsEnabled(7));        // prints 255
            io60p16.Interrupt += (sender, args) => Debug.Print("Port: " + args.Port + "  Pin: " + args.Pin);
            var timer2 = new GT.Timer(500);
            timer2.Tick += timer1 =>
            {
                Debug.Print("Tick.");
                io60p16.Write(IOPin.Port0_Pin6, false);
                Thread.Sleep(100);
                io60p16.Write(IOPin.Port0_Pin6, true);
            };
            timer2.Start();
            return;

            // Test interrupts on all pins.
            var op = io60p16.CreateOutputPort(IOPin.Port0_Pin6, true);
            //io60p16.CreateInterruptPort(IOPin.Port0_Pin0);
            //io60p16.CreateInterruptPort(IOPin.Port0_Pin1);
            //io60p16.CreateInterruptPort(IOPin.Port0_Pin2);
            //io60p16.CreateInterruptPort(IOPin.Port0_Pin3);
            //io60p16.CreateInterruptPort(IOPin.Port0_Pin4);
            //io60p16.CreateInterruptPort(IOPin.Port0_Pin5);
            ////io60p16.CreateInterruptPort(IOPin.Port0_Pin6);
            //io60p16.CreateInterruptPort(IOPin.Port0_Pin7);
            //io60p16.CreateInterruptPort(IOPin.Port1_Pin0);
            //io60p16.CreateInterruptPort(IOPin.Port1_Pin1);
            //io60p16.CreateInterruptPort(IOPin.Port1_Pin2);
            //io60p16.CreateInterruptPort(IOPin.Port1_Pin3);
            //io60p16.CreateInterruptPort(IOPin.Port1_Pin4);
            //io60p16.CreateInterruptPort(IOPin.Port1_Pin5);
            //io60p16.CreateInterruptPort(IOPin.Port1_Pin6);
            //io60p16.CreateInterruptPort(IOPin.Port1_Pin7);
            //io60p16.CreateInterruptPort(IOPin.Port2_Pin0);
            //io60p16.CreateInterruptPort(IOPin.Port2_Pin1);
            //io60p16.CreateInterruptPort(IOPin.Port2_Pin2);
            //io60p16.CreateInterruptPort(IOPin.Port2_Pin3);
            //io60p16.CreateInterruptPort(IOPin.Port3_Pin0);
            //io60p16.CreateInterruptPort(IOPin.Port3_Pin1);
            //io60p16.CreateInterruptPort(IOPin.Port3_Pin2);
            //io60p16.CreateInterruptPort(IOPin.Port3_Pin3);
            //io60p16.CreateInterruptPort(IOPin.Port3_Pin4);
            //io60p16.CreateInterruptPort(IOPin.Port3_Pin5);
            //io60p16.CreateInterruptPort(IOPin.Port3_Pin6);
            //io60p16.CreateInterruptPort(IOPin.Port3_Pin7);
            //io60p16.CreateInterruptPort(IOPin.Port4_Pin0);
            //io60p16.CreateInterruptPort(IOPin.Port4_Pin1);
            //io60p16.CreateInterruptPort(IOPin.Port4_Pin2);
            //io60p16.CreateInterruptPort(IOPin.Port4_Pin3);
            //io60p16.CreateInterruptPort(IOPin.Port4_Pin4);
            //io60p16.CreateInterruptPort(IOPin.Port4_Pin5);
            //io60p16.CreateInterruptPort(IOPin.Port4_Pin6);
            //io60p16.CreateInterruptPort(IOPin.Port4_Pin7);
            //io60p16.CreateInterruptPort(IOPin.Port5_Pin0);
            //io60p16.CreateInterruptPort(IOPin.Port5_Pin1);
            //io60p16.CreateInterruptPort(IOPin.Port5_Pin2);
            //io60p16.CreateInterruptPort(IOPin.Port5_Pin3);
            //io60p16.CreateInterruptPort(IOPin.Port5_Pin4);
            //io60p16.CreateInterruptPort(IOPin.Port5_Pin5);
            //io60p16.CreateInterruptPort(IOPin.Port5_Pin6);
            //io60p16.CreateInterruptPort(IOPin.Port5_Pin7);
            var i = io60p16.CreateInterruptPort(IOPin.Port6_Pwm0);
            i.OnInterrupt += (data1, data2, time) => Debug.Print("Zap!");
            io60p16.CreateInterruptPort(IOPin.Port6_Pwm1);
            io60p16.CreateInterruptPort(IOPin.Port6_Pwm2);
            io60p16.CreateInterruptPort(IOPin.Port6_Pwm3);
            io60p16.CreateInterruptPort(IOPin.Port6_Pwm4);
            io60p16.CreateInterruptPort(IOPin.Port6_Pwm5);
            io60p16.CreateInterruptPort(IOPin.Port6_Pwm6);
            io60p16.CreateInterruptPort(IOPin.Port6_Pwm7);
            //io60p16.CreateInterruptPort(IOPin.Port7_Pwm8);
            //io60p16.CreateInterruptPort(IOPin.Port7_Pwm9);
            //io60p16.CreateInterruptPort(IOPin.Port7_Pwm10);
            //io60p16.CreateInterruptPort(IOPin.Port7_Pwm11);
            //io60p16.CreateInterruptPort(IOPin.Port7_Pwm12);
            //io60p16.CreateInterruptPort(IOPin.Port7_Pwm13);
            //io60p16.CreateInterruptPort(IOPin.Port7_Pwm14);
            //io60p16.CreateInterruptPort(IOPin.Port7_Pwm15);

            io60p16.Interrupt += (sender, args) => Debug.Print("Port: " + args.Port + "  Pin: " + args.Pin);
            //var t2 = new GT.Timer(200);
            //t2.Tick += timer1 =>{
            //                        op.Write(false);
            //                        Thread.Sleep(20);
            //                        op.Write(true);
            //                    };
            //t2.Start();
            return;

            // Test multiple PWMs.
            pwm = io60p16.CreatePwm(PwmPin.Pwm9, 20 * 1000 * 1000, 1500 * 1000, PWM.ScaleFactor.Nanoseconds, false);
            pwm2 = io60p16.CreatePwm(PwmPin.Pwm10, 20 * 1000 * 1000, 1500 * 1000, PWM.ScaleFactor.Nanoseconds, false);
            pwm.Start();
            pwm2.Start();
            //pwm.SetPwm(7, 3, 20*1000*1000, 1500*1000);

            // Call these where you were calling .Start()
            //SetPwm(io60p16, 6, 0, 255, 19, ModulePwm.PwmClockSource.Clock_367Hz6, 7);
            //SetPwm(io60p16, 6, 1, 255, 19, ModulePwm.PwmClockSource.Clock_367Hz6, 7);

            return;

            // Test PWM.
            //var pwm2 = new Microsoft.SPOT.Hardware.PWM(Cpu.PWMChannel.PWM_7, 8000, 4000, Microsoft.SPOT.Hardware.PWM.ScaleFactor.Nanoseconds, false);
            pwm = io60p16.CreatePwm(PwmPin.Pwm14, 5000, 4000, Gadgeteer.Modules.IanLee.IO60P16.PWM.ScaleFactor.Nanoseconds, false);
            pwm.Start();
            return;

            // Test reading the drive mode for a pin.
            io60p16.SetDirection(IOPin.Port2_Pin0, PinDirection.Output);
            io60p16.SetResistorMode(IOPin.Port2_Pin0, ResistorMode.ResistivePullDown);
            io60p16.SetDirection(IOPin.Port2_Pin1, PinDirection.Output);
            io60p16.SetResistorMode(IOPin.Port2_Pin1, ResistorMode.HighImpedence);
            Debug.Print("Port2_Pin0 Drive Mode: " + io60p16.GetResistorMode(IOPin.Port2_Pin0));
            Debug.Print("Port2_Pin1 Drive Mode: " + io60p16.GetResistorMode(IOPin.Port2_Pin1));

            io60p16.WriteRegister(IO60P16Module.PORT_SELECT_REGISTER, 2);
            for (byte reg = 0x1d; reg <= 0x23; reg++)
            {
                Debug.Print(reg + ":  " + io60p16.ReadRegister(reg));
            }
            return;

            // Test 3x3x3 LED Cube.
            //_ledCube = new LedCube3(io60p16, 3
            //    // Levels
            //    , new[] { IOPin.Port2_Pin2, IOPin.Port2_Pin1, IOPin.Port2_Pin0 }
            //    // Columns
            //    , new[] { IOPin.Port7_Pwm11, IOPin.Port7_Pwm12, IOPin.Port7_Pwm13, IOPin.Port7_Pwm14, IOPin.Port7_Pwm15
            //             ,IOPin.Port6_Pwm0, IOPin.Port7_Pwm10, IOPin.Port7_Pwm9, IOPin.Port7_Pwm8 });

            //var cubeTimer = new GT.Timer(5000);
            //cubeTimer.Tick += timer1 =>
            //{
            //    var effects = new CubeEffect[]
            //                                            {
            //                                                new AsciiChar(_ledCube, "HELLO WORLD"),
            //                                                new TallyUp(_ledCube, 100),
            //                                                new TallyDown(_ledCube, 50),
            //                                                new SirenClockwise(_ledCube, 100),
            //                                                new SirenCounterclockwise(_ledCube, 100),
            //                                                new AsciiChar(_ledCube, 100, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            //                                                new SpiralUp(_ledCube, 100),
            //                                                new SpiralDown(_ledCube, 50),
            //                                                new Randomizer(_ledCube, 20)
            //                                            };
            //    foreach (var effect in effects)
            //    {
            //        effect.Start();
            //    }
            //};
            //cubeTimer.Start();
            return;

            // Test problem with Port0_Pin0 Dobova was having.
            ip0 = io60p16.CreateInterruptPort(IOPin.Port1_Pin0);
            ip0.OnInterrupt += (data1, data2, time) => Debug.Print("Exterminate!");
            op12 = io60p16.CreateOutputPort(IOPin.Port0_Pin0, false);

            var timer3 = new GT.Timer(1000);
            timer3.Tick += timer1 =>
                               {
                                   op12.Write(!op12.Read());
                                   Debug.Print("op12: " + op12.Read());
                               };
            timer3.Start();
            return;

            ip0 = io60p16.CreateInterruptPort(IOPin.Port0_Pin0);
            ip0.OnInterrupt += (data1, data2, time) => { Debug.Print("P0P0 Hello!"); };
            op12 = io60p16.CreateOutputPort(IOPin.Port7_Pwm8, false);
            var t = new GT.Timer(1000);
            t.Tick += timer1 => op12.Write(!op12.Read());
            t.Start();
            return;

            // Test reading the value of an individual pin.
            io60p16.SetDirection(IOPin.Port7_Pwm8, PinDirection.Output);
            io60p16.SetDirection(IOPin.Port7_Pwm9, PinDirection.Output);
            io60p16.SetDirection(IOPin.Port7_Pwm10, PinDirection.Output);
            io60p16.Write(IOPin.Port7_Pwm8, true);
            io60p16.Write(IOPin.Port7_Pwm9, false);
            io60p16.Write(IOPin.Port7_Pwm10, true);
            var p = io60p16.Read(port:7);
            Debug.Print("Port Value: " + p);
            Debug.Print("Pwm8: " + io60p16.Read(IOPin.Port7_Pwm8));
            Debug.Print("Pwm9: " + io60p16.Read(IOPin.Port7_Pwm9));
            Debug.Print("Pwm10: " + io60p16.Read(IOPin.Port7_Pwm10));
            return;

            // Test reading the pin directions on a port.
            io60p16.WriteRegister(0x18, 7);
            Debug.Print("0x1C = " + io60p16.ReadRegister(0x1c).ToString());

            io60p16.SetDirection(7, 0x0f);
            io60p16.WriteRegister(0x18, 7);
            Debug.Print("0x1C = " + io60p16.ReadRegister(0x1c).ToString());

            io60p16.SetDirection(IOPin.Port7_Pwm15, PinDirection.Input);
            io60p16.WriteRegister(0x18, 7);
            Debug.Print("0x1C = " + io60p16.ReadRegister(0x1c).ToString());

            io60p16.SetDirection(IOPin.Port7_Pwm15, PinDirection.Output);
            io60p16.WriteRegister(0x18, 7);
            Debug.Print("0x1C = " + io60p16.ReadRegister(0x1c).ToString());
            return;

            // Blink an LED.
            op15 = io60p16.CreateOutputPort(IOPin.Port7_Pwm10, true);
            Debug.Print("0x1C = " + io60p16.ReadRegister(0x1c).ToString());
            var timer = new GT.Timer(1000);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            timer.Start();
            return;

            //op12 = io60p16.CreateOutputPort(IOPin.Port7_Pwm12, false);
            //op12.Write(true);
            //op12.Write(false);

            io60p16.RestoreFactoryDefaults();
            io60p16.Reset();
            //io60p16.SetPortMode(7, DriveMode.ResistivePullUp);
            //io60p16.SetPortMode(7, DriveMode.HighImpedence);
            //io60p16.SetPortMode(7, DriveMode.OpenDrainHigh);
            io60p16.SetResistorMode(7, ResistorMode.ResistivePullDown);
            io60p16.SetDirection((byte) 7, (byte) PinDirection.Output);

            var blinker = new GT.Timer(1000);
            blinker.Tick += timer1 =>
                                {
                                    io60p16.Write(7, 0x00);
                                    Thread.Sleep(500);
                                    io60p16.Write(7, 0x04);
                                    Debug.Print("Set: 4 \tRead: " + io60p16.Read(7).ToString());
                                };
            blinker.Start();
            return;

            var op = new OutputPort[8];
            for (var n = 0; n < 8; n++)
            {
                op[n] = io60p16.CreateOutputPort((IOPin) ((byte) IOPin.Port7_Pwm8 + n), false);
            }

            // Turn them all off one at a time.
            for (var n = 0; n < 8; n++)
            {
                op[n].Write(false);
            }

            // Turn them all on one at a time.
            for (var n = 0; n < 8; n++)
            {
                op[n].Write(true);
            }

            io60p16.Write(7, 0x00);                  // Turn off an entire bank of pins at once.
            io60p16.Write(7, 0xff);                 // Turn on an entire bank of pins at once.

//            joystick.JoystickPressed += new Joystick.JoystickEventHandler(joystick_JoystickPressed);
            const byte port = 7;
            const byte pin = 0;



            //io60p16.SetPwm(port, pin, 692 * 1000, 20000);
            //io60p16.SetPwm(port, pin, 8 * 1000, 4000);
            //io60p16.SetPwm(port, pin, 6 * 1000, 3000);
            //io60p16.SetPwm(port, pin, 2 * 1000, 1000);
            //io60p16.SetPwm(port, pin, 1 * 1000, 500);
            //io60p16.SetPwm(port, pin, 150, 75);
            //io60p16.SetPwm(port, pin, 10, 5);

            // Test max period for all the clocks.
            //io60p16.SetPwm(port, pin, 255, 100, PwmClockSource.Clock_367Hz6);
            //io60p16.SetPwm(port, pin, 255, 100, PwmClockSource.Clock_32kHz);
            //io60p16.SetPwm(port, pin, 255, 100, PwmClockSource.Clock_93kHz75);
            //io60p16.SetPwm(port, pin, 255, 100, PwmClockSource.Clock_1Mhz5);
            //io60p16.SetPwm(port, pin, 255, 100, PwmClockSource.Clock_24MHz);

            // Test a stepper motor.
            //period = 255;
            //while (true)
            //{
            //    io60p16.SetPwm(port, pin, period, 45, PwmClockSource.Clock_32kHz);
            //    Thread.Sleep(1000);
            //    io60p16.SetPwm(port, pin, period, 55, PwmClockSource.Clock_32kHz);
            //    Thread.Sleep(1000);
            //    io60p16.SetPwm(port, pin, period, 64, PwmClockSource.Clock_32kHz);
            //    Thread.Sleep(1000);
            //}
*/
        }

        private void Test_Thread()
        {
            //lcd.Clear();
            //Thread.Sleep(20);
            //lcd.SetCursor(0, 0);
            //lcd.PrintString("Program Started");
            Thread.Sleep(100);
            while (true)
            {
                Debug.Print("InterruptEnable:  " + io60p16.GetInterruptsEnabled(7));

                ip0.ClearInterrupt(); //// <---------------------------with this line the INT works fine !!!
                op12.Write(true);
                Thread.Sleep(200);
                op12.Write(false);
                Thread.Sleep(500);
            }
        }

        //void joystick_JoystickPressed(Joystick sender, Joystick.JoystickState state)
        //{
        //    pulseWidth += 0x01;
        //    //io60p16.SetPwm(7, 0, period, pulseWidth);
        //}

        void timer_Tick(GT.Timer timer)
        {
            op15.Write(!op15.Read());
        }
    }
}
