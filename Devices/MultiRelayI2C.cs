using System;
using System.Device.I2c;

namespace GroveDev
{

    public class MultiRelay : IDisposable
    {
        private const int I2cBusId = 1;
        private const byte DefaultAddress = 0x12;

        private const byte CMD_CHANNEL_CTRL = 0x10;
        private const byte CMD_SAVE_I2C_ADDR = 0x11;
        private const byte CMD_READ_I2C_ADDR = 0x12;
        private const byte CMD_READ_FIRMWARE_VER = 0x13;

        private I2cDevice _device;
        public byte Address { get; private set; }
        public byte ChannelStatus { get; private set; }

        public MultiRelay(byte i2cAddress = DefaultAddress)
        {
            Address = i2cAddress;
            var settings = new I2cConnectionSettings(I2cBusId, Address);
            _device = I2cDevice.Create(settings);
            ChannelStatus = 0;
        }

        public void WriteChannelStatus(byte status)
        {
            if (status > 0x0F) throw new ArgumentOutOfRangeException(nameof(status), "Status must be 0-15 (4 bits).");
            Span<byte> writeBuf = stackalloc byte[] { CMD_CHANNEL_CTRL, status };
            _device.Write(writeBuf);
            ChannelStatus = status;
            Console.WriteLine($"Channel status set to: {status}");
        }

        public byte ReadChannelStatus()
        {
            Span<byte> readBuf = stackalloc byte[1];
            _device.WriteRead(new byte[] { CMD_CHANNEL_CTRL }, readBuf);
            ChannelStatus = readBuf[0];
            return ChannelStatus;
        }

        public void SetI2cAddress(byte newAddr)
        {
            // Some modules require special sequence; this writes the save-address command.
            Span<byte> writeBuf = stackalloc byte[] { CMD_SAVE_I2C_ADDR, newAddr };
            _device.Write(writeBuf);
            Address = newAddr;
            Console.WriteLine($"New I2C address set: 0x{newAddr:X2}");
        }

        public byte ReadI2cAddress()
        {
            Span<byte> readBuf = stackalloc byte[1];
            _device.WriteRead(new byte[] { CMD_READ_I2C_ADDR }, readBuf);
            return readBuf[0];
        }

        public byte GetFirmwareVersion()
        {
            Span<byte> readBuf = stackalloc byte[1];
            _device.WriteRead(new byte[] { CMD_READ_FIRMWARE_VER }, readBuf);
            return readBuf[0];
        }

        public void Dispose()
        {
            _device?.Dispose();
            _device = null;
        }
    }

}