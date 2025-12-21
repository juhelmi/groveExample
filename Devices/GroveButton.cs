using System;
using System.Device.Gpio;

namespace GroveHatExample. Devices
{
    public class GroveButton : IDisposable
    {
        private readonly GpioController _gpio;
        private readonly int _pin;
        
        public event EventHandler?  Pressed;
        public event EventHandler? Released;
        
        public GroveButton(GpioController gpio, int pin)
        {
            _gpio = gpio;
            _pin = pin;
            _gpio.OpenPin(_pin, PinMode. InputPullUp);
            
            // Register event handlers
            _gpio.RegisterCallbackForPinValueChangedEvent(
                _pin, 
                PinEventTypes. Falling, 
                OnButtonPressed);
            _gpio.RegisterCallbackForPinValueChangedEvent(
                _pin, 
                PinEventTypes.Rising, 
                OnButtonReleased);
        }
        
        private void OnButtonPressed(object sender, PinValueChangedEventArgs e)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }
        
        private void OnButtonReleased(object sender, PinValueChangedEventArgs e)
        {
            Released?.Invoke(this, EventArgs.Empty);
        }
        
        public bool IsPressed => _gpio.Read(_pin) == PinValue.Low;
        
        public void Dispose()
        {
            _gpio.UnregisterCallbackForPinValueChangedEvent(_pin, OnButtonPressed);
            _gpio. UnregisterCallbackForPinValueChangedEvent(_pin, OnButtonReleased);
            _gpio.ClosePin(_pin);
        }
    }
}
