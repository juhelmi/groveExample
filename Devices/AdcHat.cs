using System;
using System.Device.I2c;

// Stype is from Python library grove.adc

namespace GroveDev
{   
    enum AdcChannel
    {
        A0 = 0,
        A1 = 1,
        A2 = 2,
        A3 = 3,
        A4 = 4,
        A6 = 6
    }

public class GroveAdc : IDisposable
{
    // Replace these with the actual values from your Python environment
    public const int RPI_ADC_ADDRESS = 0x08;
    public const int RPI_HAT_PID = 0x0004;         // example PID
    public const int RPI_ZERO_HAT_PID = 0x0005;    // example PID
    public const string RPI_HAT_NAME = "Grove Base Hat";
    public const string RPI_ZERO_HAT_NAME = "Grove Base Hat Zero";

    // ADC channel mappings
    public const int A0 = 0;
        public const int A1 = 1;
        public const int A2 = 2;
        public const int A3 = 3;
        public const int A4 = 4;
        public const int A6 = 6;

    private readonly I2cDevice _device;
    private bool _disposed;

    /// <summary>
    /// If true, the two bytes read from the device are interpreted as little-endian.
    /// Set to false if your device returns big-endian word order.
    /// Default assumes little-endian (low byte first).
    /// </summary>
    public bool ReadIsLittleEndian { get; set; } = true;

    public GroveAdc(int address = RPI_ADC_ADDRESS, int busId = 1)
    {
        var settings = new I2cConnectionSettings(busId, address);
        _device = I2cDevice.Create(settings);
    }

    /// <summary>
    /// Read raw ADC value (12-bit) from channel 0..7
    /// </summary>
    public int ReadRaw(int channel)
    {
        if (channel < 0 || channel > 7) throw new ArgumentOutOfRangeException(nameof(channel));
        int addr = 0x10 + channel;
        return ReadRegister(addr);
    }

    /// <summary>
    /// Read input voltage in mV from channel 0..7
    /// </summary>
    public int ReadVoltage(int channel)
    {
        if (channel < 0 || channel > 7) throw new ArgumentOutOfRangeException(nameof(channel));
        int addr = 0x20 + channel;
        return ReadRegister(addr);
    }

    /// <summary>
    /// Read ratio (input / output) in 0.1% units from channel 0..7
    /// </summary>
    public int Read(int channel)
    {
        if (channel < 0 || channel > 7) throw new ArgumentOutOfRangeException(nameof(channel));
        int addr = 0x30 + channel;
        return ReadRegister(addr);
    }

    /// <summary>
    /// Hat name based on PID register (0x00)
    /// </summary>
    public string Name
    {
        get
        {
            int id = ReadRegister(0x00);
            if (id == RPI_HAT_PID) return RPI_HAT_NAME;
            if (id == RPI_ZERO_HAT_PID) return RPI_ZERO_HAT_NAME;
            return $"Unknown (PID=0x{id:X4})";
        }
    }

    /// <summary>
    /// Firmware version from register 0x02
    /// </summary>
    public int Version => ReadRegister(0x02);

    /// <summary>
    /// Read a 16-bit register value from the device.
    /// This writes the register address (single byte) then reads two bytes.
    /// </summary>
    public int ReadRegister(int register)
    {
        if (register < 0 || register > 0xFF) throw new ArgumentOutOfRangeException(nameof(register));

        try
        {
            // Write the register pointer (single byte)
            _device.WriteByte((byte)register);

            // Read two bytes
            Span<byte> buffer = stackalloc byte[2];
            _device.Read(buffer);

            // Combine bytes into 16-bit value
            int value;
            if (ReadIsLittleEndian)
            {
                // buffer[0] = low byte, buffer[1] = high byte
                value = buffer[0] | (buffer[1] << 8);
            }
            else
            {
                // big-endian: buffer[0] = high byte, buffer[1] = low byte
                value = (buffer[0] << 8) | buffer[1];
            }

            return value & 0xFFFF;
        }
        catch (Exception ex) when (ex is System.IO.IOException || ex is InvalidOperationException)
        {
            // Mirror the Python behavior: print helpful message and rethrow or wrap
            Console.Error.WriteLine($"I2C error: Check whether I2C is enabled and {RPI_HAT_NAME} or {RPI_ZERO_HAT_NAME} is inserted.");
            throw;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _device?.Dispose();
            _disposed = true;
        }
    }
}

}
