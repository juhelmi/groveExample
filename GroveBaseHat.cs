using System;
using System.Device.Gpio;
using System. Device.I2c;

using GroveDev;

namespace groveExample
{
    public class GroveBaseHat :  IDisposable
    {
        private readonly GpioController _gpio;
        private readonly GroveAdc _adc;
        private readonly Ads1115Wrapper _ads1115;
        private readonly Mcp9600Wrapper _mcp9600;
        private readonly TCA9548AWrapper _i2cMux;
        private readonly MultiRelay _multiRelay;
        
        // Grove digital port mappings (BCM GPIO numbers)
        public const int D5 = 5;
        public const int D16 = 16;
        public const int D18 = 18;
        public const int D22 = 22;
        public const int D24 = 24;
        public const int D26 = 26;
        public const int PWM = 12;
        public const int PWM1 = 13;
        
        public GroveBaseHat()
        {
            _gpio = new GpioController();
            _i2cMux = new TCA9548AWrapper();
            //_i2cDevice = 
            _adc = new GroveAdc();
            _ads1115 = new Ads1115Wrapper();
            _mcp9600 = new Mcp9600Wrapper();
            _multiRelay = new MultiRelay();

            Console.WriteLine( $"Grove Base Hat has ADC {_adc.Name}");
        }
        
        public GpioController Gpio => _gpio;
        public GroveAdc Adc => _adc;
        public Ads1115Wrapper Ads1115Wrapper => _ads1115;
        public Mcp9600Wrapper Mcp9600Wrapper => _mcp9600;
        public TCA9548AWrapper I2cMux => _i2cMux;
        public MultiRelay MultiRelay => _multiRelay;
        public void TestMux()
        {
            _i2cMux.TestUse();
        }
        public void MuxAllOn()
        {
            _i2cMux.SelectChannel(TcaChannels.TCA_CHANNEL_ALL);
        }
        
        public void Dispose()
        {
            _gpio?.Dispose();
            _adc?.Dispose();
            _ads1115?.Dispose();
            _mcp9600?.Dispose();
            _i2cMux?.Dispose();
            _multiRelay?.Dispose();
        }
    }
}
