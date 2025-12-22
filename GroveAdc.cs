using System;
using System.Device.I2c;

namespace GroveHatExample
{
    public class GroveAdc : IDisposable
    {
        private readonly I2cDevice _i2cDevice;
        // private const byte ADC_ADDRESS = 0x04;
        private const byte ADC_ADDRESS = 0x08; // Corrected I2C address for Grove ADC
        
        // ADC channel mappings
        public const int A0 = 0;
        public const int A1 = 1;
        public const int A2 = 2;
        public const int A3 = 3;
        public const int A4 = 4;
        public const int A6 = 6;
        
        public GroveAdc()
        {
            var settings = new I2cConnectionSettings(1, ADC_ADDRESS);
            _i2cDevice = I2cDevice.Create(settings);
        }
        
        public int ReadValue(int channel)
        {
            if (channel < 0 || channel > 7)
                throw new ArgumentException("Channel must be between 0 and 7");
            
            byte[] writeBuffer = new byte[] { (byte)channel };
            byte[] readBuffer = new byte[2];
            
            _i2cDevice.Write(writeBuffer);
            System.Threading.Thread.Sleep(1); // Small delay for ADC conversion
            _i2cDevice.Read(readBuffer);
            
            // Combine bytes (12-bit ADC value)
            int value = (readBuffer[0] << 8) | readBuffer[1];
            return value & 0x0FFF; // Mask to 12 bits
        }
        
        public double ReadVoltage(int channel, double referenceVoltage = 3.3)
        {
            int rawValue = ReadValue(channel);
            return (rawValue / 4095.0) * referenceVoltage;
        }
        
        public void Dispose()
        {
            _i2cDevice?.Dispose();
        }
    }
}
