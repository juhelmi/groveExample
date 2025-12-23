using System.Device.I2c;
using Iot.Device.Ads1115;
using UnitsNet;

namespace GroveDev
{
    public class Ads1115Wrapper : IDisposable
    {

        private readonly I2cDevice _device;
        private readonly Ads1115 _adc;

        public const int DefaultI2cAddress = 0x48;

        public Ads1115Wrapper(int address = DefaultI2cAddress, int busId = 1)
        {
            var settings = new I2cConnectionSettings(busId, address);
            _device = I2cDevice.Create(settings);
            _adc = new Ads1115(_device, InputMultiplexer.AIN0_AIN1, MeasuringRange.FS4096, DataRate.SPS475);
        
            // MeasuringRange.FS6144 provides ±6.144V range with 0.1875mV resolution
            // MeasuringRange.FS4096 provides ±4.096V range with 0.125mV resolution
            // DataRate.SPS475 provides 475 samples per second
            //_adc = new Ads1115(_device, Iot.Device.Ads1115.Ads1115.InputMultiplexer.AIN0_AIN1, MeasuringRange.FS4096, DataRate.SPS475);
        }

        public ElectricPotential ReadVoltage(int channel)
        {
            return _adc.ReadVoltage((InputMultiplexer)channel);
        }
        public ElectricPotential ReadVoltage(InputMultiplexer channel)
        {
            return _adc.ReadVoltage(channel);
        }

        public short ReadRaw(InputMultiplexer channel)
        {
            //  public short ReadRaw(InputMultiplexer inputMultiplexer, MeasuringRange measuringRange, DataRate dataRate)
            return _adc.ReadRaw(channel);
        }

        public void DemoUse()
        {
            // create I2C device (bus 1) and Ads1115 instance (default address 0x48)
            //var settings = new I2cConnectionSettings(1, Ads1115Wrapper.DefaultI2cAddress);
            //var i2c = I2cDevice.Create(settings);
            //using var adc = new Ads1115Wrapper(i2c);

            // single-ended read from AIN0
            ElectricPotential volts = _adc.ReadVoltage(InputMultiplexer.AIN0);
            short raw = _adc.ReadRaw(InputMultiplexer.AIN0);
            Console.WriteLine($"Voltage: {volts:F4} V, Raw: {raw}");
        }

        public void Dispose()
        {
            _adc?.Dispose();
        }
    }

}

