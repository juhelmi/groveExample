using System;

using GroveDev;

namespace GroveHatExample.Devices
{
    public class GroveJoystick :  IDisposable
    {
        private readonly GroveAdc _adc;
        private readonly int _channelX;
        private readonly int _channelY;
        
        public GroveJoystick(GroveAdc adc, int channelX, int channelY)
        {
            _adc = adc;
            _channelX = channelX;
            _channelY = channelY;
        }
        
        public int ReadXRaw() => _adc.ReadRaw(_channelX);
        public int ReadYRaw() => _adc.ReadRaw(_channelY);
        
        public double ReadXVoltage() => _adc.ReadVoltage(_channelX)/1000.0;
        public double ReadYVoltage() => _adc.ReadVoltage(_channelY)/1000.0;
        
        // Returns normalized values (-1.0 to 1.0)
        public (double x, double y) ReadNormalized()
        {
            int xRaw = ReadXRaw();
            int yRaw = ReadYRaw();
            
            // Center position is around 2048 (middle of 0-4095 range)
            double x = (xRaw - 2048.0) / 2048.0;
            double y = (yRaw - 2048.0) / 2048.0;
            
            // Clamp to -1.0 to 1.0
            x = Math.Clamp(x, -1.0, 1.0);
            y = Math.Clamp(y, -1.0, 1.0);
            
            return (x, y);
        }
        
        // Returns direction as enum
        public JoystickDirection GetDirection(double deadzone = 0.3)
        {
            var (x, y) = ReadNormalized();
            
            if (Math.Abs(x) < deadzone && Math.Abs(y) < deadzone)
                return JoystickDirection.Center;
            
            // Determine primary direction
            if (Math.Abs(x) > Math.Abs(y))
            {
                return x > 0 ? JoystickDirection.Right : JoystickDirection.Left;
            }
            else
            {
                return y > 0 ? JoystickDirection.Up : JoystickDirection.Down;
            }
        }
        
        public void Dispose()
        {
            // ADC is shared, don't dispose here
        }
    }
    
    public enum JoystickDirection
    {
        Center,
        Up,
        Down,
        Left,
        Right
    }
}
