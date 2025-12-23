using System;
using System.Device.I2c;

/*
 TCA9548A I2C Multiplexer
 Datasheet: https://www.ti.com/lit/ds/symlink/tca9548a.pdf
 Python implementation: https://github.com/adafruit/Adafruit_CircuitPython_TCA9548A
*/
/**
Minimal implementation example:
using System;
using System.Device.I2c;

public class Tca9548a
{
    private readonly I2cDevice _device;

    public Tca9548a(int busId, int address = 0x70)
    {
        _device = I2cDevice.Create(new I2cConnectionSettings(busId, address));
    }

    public void SelectChannel(int channel)
    {
        if (channel < 0 || channel > 7)
            throw new ArgumentOutOfRangeException(nameof(channel));

        byte mask = (byte)(1 << channel);
        _device.WriteByte(mask);
    }

    public void DisableAll()
    {
        _device.WriteByte(0x00);
    }
}

*/


namespace GroveDev
{
    public enum TcaChannels
    {
        TCA_CHANNEL_0=0x1,
        TCA_CHANNEL_1=0x2,
        TCA_CHANNEL_2=0x4,
        TCA_CHANNEL_3=0x8,
        TCA_CHANNEL_4=0x10,
        TCA_CHANNEL_5=0x20,
        TCA_CHANNEL_6=0x40,
        TCA_CHANNEL_7=0x80,
        TCA_CHANNEL_ALL=0xFF    
    }

    public class TCA9548AWrapper : IDisposable
    {
        private readonly I2cDevice _device;
        private int _BusId = 1;
        private int _TcaAddress = 0x70;
        
        public TCA9548AWrapper(int busId = 1, int tcaAddress = 0x70)
        {
            _BusId = busId;
            _TcaAddress = tcaAddress;
            var settings = new I2cConnectionSettings(_BusId, _TcaAddress);
            _device = I2cDevice.Create(settings);
        }
        public void TestUse()
        {
            // Select channel 0 or 2 (for example)
            SelectChannel(0);

            // Example: read register 0xD0 from a sensor behind channel 2
            //var sensor = I2cDevice.Create(new I2cConnectionSettings(_BusId, _TcaAddress));
            _device.WriteByte(0xD0);
            byte id = _device.ReadByte();

            Console.WriteLine($"Mux ID: 0x{id:X2}");
            //SelectChannel(TcaChannels.TCA_CHANNEL_ALL);
        }

        public void SelectChannel(TcaChannels channel)
        {
            SelectChannel((int)channel);
        }
        public void SelectChannel(int channel)
        {
            if (channel < 0 || channel > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(channel));

            var mux = I2cDevice.Create(new I2cConnectionSettings(_BusId, _TcaAddress));

            // Each bit corresponds to a channel
            byte command = (byte)(1 << channel);

            mux.WriteByte(command);

            Console.WriteLine($"Selected TCA9548A channel {channel}");
        }

        /// <summary> 
        /// Runs an action while a specific channel is active. /// 
        /// Automatically restores the previous state. /// 
        /// </summary> 
        public void UseChannel(int channel, Action action) 
        { 
            if (action is null) 
                throw new ArgumentNullException(nameof(action)); 
            SelectChannel(channel); 
            action(); 
            DisableAll(); 
        } 
        /// <summary> /// 
        /// Runs a function while a specific channel is active. /// 
        /// Returns the function result. /// 
        /// </summary> 
        public T UseChannel<T>(int channel, Func<T> func) 
        {
            ArgumentNullException.ThrowIfNull(func);
            SelectChannel(channel); 
            T result = func(); 
            DisableAll(); 
            return result; 
        }

        public void DisableAll() 
        { 
            _device.WriteByte(0x00); 
        }
        public void Dispose()
        {
            _device?.Dispose();
        }

    }

}