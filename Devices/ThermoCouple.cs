using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Net;

// MCP9600 Thermocouple Amplifier
/*
 Datasheet: https://ww1.microchip.com/downloads/en/DeviceDoc/20005602C.pdf
 Application Note: https://ww1.microchip.com/downloads/en/AppNotes/00002445B.pdf
*/
/* Test with
using var sensor = new GroveThermocoupleAmpMCP9600();
sensor.SetFilterCoefficients(GroveThermocoupleAmpMCP9600.FILT_OFF);
double tempC = sensor.ReadTemperature();
Console.WriteLine($"Temperature: {tempC:F2} °C");
*/


namespace GroveDev
{
    public class GroveThermocoupleAmpMCP9600 : IDisposable
    {
        // Registers / constants
        private const byte VERSION_ID_REG_ADDR = 0x20;
        private const byte THERM_SENS_CFG_REG_ADDR = 0x05;
        private const byte DEVICE_CFG_REG_ADDR = 0x06;
        private const byte HOT_JUNCTION_REG_ADDR = 0x00;

        // Example config masks (add others as needed)
        public const byte FILT_OFF = 0;
        public const byte COLD_JUNC_RESOLUTION_0_625 = 0 << 7;
        public const byte ADC_14BIT_RESOLUTION = 2 << 5;
        public const byte BURST_1_SAMPLE = 0 << 2;
        public const byte NORMAL_OPERATION = 0;

        private readonly I2cDevice _device;
        private byte _junc = HOT_JUNCTION_REG_ADDR;
        private byte _juncRes = COLD_JUNC_RESOLUTION_0_625;

        public GroveThermocoupleAmpMCP9600(int busId = 1, byte address = 0x60)
        {
            var settings = new I2cConnectionSettings(busId, address);
            _device = I2cDevice.Create(settings);
        }

        private byte ReadByte(byte reg)
        {
            Span<byte> write = stackalloc byte[] { reg };
            Span<byte> read = stackalloc byte[1];
            _device.WriteRead(write, read);
            return read[0];
        }

        private void WriteByte(byte reg, byte value)
        {
            Span<byte> buf = stackalloc byte[] { reg, value };
            _device.Write(buf);
        }

        private byte[] ReadBlock(byte reg, int length)
        {
            Span<byte> write = stackalloc byte[] { reg };
            byte[] read = new byte[length];
            _device.WriteRead(write, read);
            return read;
        }

        public byte[] ReadVersion()
        {
            return ReadBlock(VERSION_ID_REG_ADDR, 2);
        }

        public void SetFilterCoefficients(byte coeff)
        {
            byte data = ReadByte(THERM_SENS_CFG_REG_ADDR);
            data = (byte)((data & 0xF8) | (coeff & 0x07));
            WriteByte(THERM_SENS_CFG_REG_ADDR, data);
        }

        public void SetColdJunctionResolution(byte juncRes)
        {
            byte data = ReadByte(DEVICE_CFG_REG_ADDR);
            data = (byte)((data & 0x7F) | (juncRes & 0x80));
            WriteByte(DEVICE_CFG_REG_ADDR, data);
            _juncRes = juncRes;
        }

        public void SetAdcResolution(byte res)
        {
            byte data = ReadByte(DEVICE_CFG_REG_ADDR);
            data = (byte)((data & 0x9F) | (res & 0x60));
            WriteByte(DEVICE_CFG_REG_ADDR, data);
        }

        public void SetBurstSamples(byte samp)
        {
            byte data = ReadByte(DEVICE_CFG_REG_ADDR);
            data = (byte)((data & 0xE3) | (samp & 0x1C));
            WriteByte(DEVICE_CFG_REG_ADDR, data);
        }

        public (byte config1, byte config2) GetConfig()
        {
            return (ReadByte(DEVICE_CFG_REG_ADDR), ReadByte(THERM_SENS_CFG_REG_ADDR));
        }

        public void SetThermType(byte thermType)
        {
            byte data = ReadByte(THERM_SENS_CFG_REG_ADDR);
            data = (byte)((data & 0x8F) | (thermType & 0x70));
            WriteByte(THERM_SENS_CFG_REG_ADDR, data);
        }

        public double ReadTemperature()
        {
            // Read two bytes from the selected junction register
            byte[] raw = ReadBlock(_junc, 2);
            // Big-endian -> convert to signed 16-bit
            ushort rawU = (ushort)((raw[0] << 8) | raw[1]);
            short signed = (short)rawU;
            double temperature = signed / 16.0;
            return temperature;
        }

        public void Dispose() => _device?.Dispose();
    }

}
