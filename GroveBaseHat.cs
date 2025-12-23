using System;
using System.Device.Gpio;
using System. Device.I2c;

using GroveDev;

namespace GroveHatExample
{
    public class GroveBaseHat :  IDisposable
    {
        private readonly GpioController _gpio;
        // I2c bus for ADS1115
        //private readonly I2cDevice _i2cDevice;
        private readonly GroveAdc _adc;
        private readonly Ads1115Wrapper _ads1115;
        
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
            //_i2cDevice = 
            _adc = new GroveAdc();
            _ads1115 = new Ads1115Wrapper();
            Console.WriteLine( $"Grove Base Hat has ADC {_adc.Name}");
        }
        
        public GpioController Gpio => _gpio;
        public GroveAdc Adc => _adc;
        public Ads1115Wrapper Ads1115Wrapper => _ads1115;
        
        public void Dispose()
        {
            _gpio?.Dispose();
            _adc?. Dispose();
        }
    }
}
