using System;
using System.Device.Gpio;

namespace GroveHatExample. Devices
{
    public class GroveLed : IDisposable
    {
        private readonly GpioController _gpio;
        private readonly int _pin;
        
        public GroveLed(GpioController gpio, int pin)
        {
            _gpio = gpio;
            _pin = pin;
            _gpio.OpenPin(_pin, PinMode.Output);
            Off();
        }
        
        public void On()
        {
            _gpio.Write(_pin, PinValue.High);
        }
        
        public void Off()
        {
            _gpio.Write(_pin, PinValue.Low);
        }
        
        public void Toggle()
        {
            var currentState = _gpio.Read(_pin);
            _gpio.Write(_pin, currentState == PinValue.High ?  PinValue.Low : PinValue.High);
        }
        
        public bool IsOn => _gpio.Read(_pin) == PinValue.High;
        
        public void Dispose()
        {
            Off();
            _gpio. ClosePin(_pin);
        }
    }
}
