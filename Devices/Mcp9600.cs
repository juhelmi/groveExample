using System;
using System.Device.I2c;
using System.Threading;


namespace GroveDev
{
    public class Mcp9600Wrapper
    {
        private readonly I2cDevice _device;
        private bool _works = false;

        const int I2C_BUS = 1; 
        const int MCP9600_ADDR = 0x60; 
        const byte REG_HOT_JUNCTION = 0x00; 
        const byte REG_COLD_JUNCTION = 0x02; 
        const byte REG_CONFIG = 0x06;

        public Mcp9600Wrapper(int address = MCP9600_ADDR, int busId = I2C_BUS)
        {
            var settings = new I2cConnectionSettings(busId, address);
            _device = I2cDevice.Create(settings);
            Initialize();
        }

        private void Initialize()
        {
            if (!_works)
                return;
            // Example initialization code
            WriteByteReg(REG_CONFIG, 0x00); // Set configuration register
            Thread.Sleep(100); // Wait for device to stabilize
            Console.WriteLine($"Config reg: 0x{ReadByteReg(REG_CONFIG):X2}");
            WriteByteReg(REG_CONFIG, 0x43); // Normal, CJ 0.0625, 18-bit, 1 Hz (esimerkki)
            Console.WriteLine($"Config reread: 0x{ReadByteReg(REG_CONFIG):X2}");
        }

        public void DemoUse()
        {
            if (!_works)
                return;
            double hotJunctionTemp = ReadTemp(REG_HOT_JUNCTION);
            double coldJunctionTemp = ReadTemp(REG_COLD_JUNCTION);
            Console.WriteLine($"Hot Junction Temp: {hotJunctionTemp:F2} °C");
            Console.WriteLine($"Cold Junction Temp: {coldJunctionTemp:F2} °C");
        }

        // Add methods to read temperature, configure the device, etc.
        public double ReadTemperature()
        {
            if (!_works)
                return double.NaN;
            // Example implementation to read temperature register
            Span<byte> writeBuffer = [0x00]; // Temperature register
            Span<byte> readBuffer = stackalloc byte[2];
            _device.WriteRead(writeBuffer, readBuffer);

            // Convert the two bytes to temperature value
            int tempRaw = (readBuffer[0] << 8) | readBuffer[1];
            double temperature = tempRaw * 0.0625; // Example conversion factor

            return temperature;
        }

        public double ReadTemp(byte reg)
        {
            Span<byte> write = [reg];
            Span<byte> read = stackalloc byte[2];
            _device.WriteRead(write, read);

            // MCP9600 is big-endian; SMBus read_word_data palautti little-endian -> swap
            int raw = (read[0] << 8) | read[1];

            // Signed 16-bit
            if ((raw & 0x8000) != 0)
                raw -= 1 << 16;

            // LSB = 0.0625 °C (kun CJ-resoluutio 0.0625 on valittu)
            return raw * 0.0625;

        }

        private double ReadByteReg(byte reg)
        {
            Span<byte> write = [reg];
            Span<byte> read = stackalloc byte[1];
            _device.WriteRead(write, read);
            return read[0];
        }

        private void WriteByteReg(byte reg, byte value)
        {
            Span<byte> data = [reg, value];
            _device.Write(data);
        }

        public void Dispose()
        {
            _device?.Dispose();
        }
    }
}


