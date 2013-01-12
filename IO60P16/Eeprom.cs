using System;
using Gadgeteer.Interfaces;
using Microsoft.SPOT;

namespace Gadgeteer.Modules.GHIElectronics.IO60P16
{
    internal class Eeprom
    {
        private const byte DEVICE_ADDRESS = 0x40;

#if HARDWARE_I2C
        private readonly GTI.I2CBus _i2c;
#else
        private readonly SoftwareI2C _i2c;
#endif

#if HARDWARE_I2C
        public Eeprom(GTI.I2CBus i2c)
#else
        public Eeprom(SoftwareI2C i2c)
#endif
        {
            _i2c = i2c;
        }

        private void WriteRegister(byte reg, byte value)
        {
            byte[] data = new[] { reg, value };

#if HARDWARE_I2C
            i2c.Write(data, 1000);
#else
            _i2c.Write(DEVICE_ADDRESS, data);
#endif
        }

        protected void Enable()
        {
            _i2c.Write(DEVICE_ADDRESS, new byte[] {0x1,
                                                   (byte)'C',
                                                   (byte)'M',
                                                   (byte)'S'});     // Select the enable register.
            _i2c.Write(DEVICE_ADDRESS, new byte[] {0x02});          // Enable EEPROM, disable WD pin.
        }

        public byte[] Read(byte startAddress, byte bytesToRead)
        {
            var data = new byte[bytesToRead];

            // Read data...

            return data;
        }

        public void Write(byte startAddress, byte[] data)
        {
            
        }
    }
}
