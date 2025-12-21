using System;
using System.Threading;
using System.Threading.Tasks;
using GroveHatExample;
using GroveHatExample.Devices;
using Microsoft.Extensions.Logging;

namespace GroveHatExample
{
    public class AdvancedGroveExample
    {
        private readonly ILogger<AdvancedGroveExample> _logger;
        private readonly GroveBaseHat _hat;
        private readonly GroveLed _led1;
        private readonly GroveLed _led2;
        private readonly GroveButton _button;
        private readonly GroveJoystick _joystick;
        private readonly CancellationTokenSource _cts;
        
        public AdvancedGroveExample(ILogger<AdvancedGroveExample> logger)
        {
            _logger = logger;
            _hat = new GroveBaseHat();
            _cts = new CancellationTokenSource();
            
            // Initialize devices
            _led1 = new GroveLed(_hat. Gpio, GroveBaseHat.D5);
            _led2 = new GroveLed(_hat.Gpio, GroveBaseHat.D16);
            _button = new GroveButton(_hat.Gpio, GroveBaseHat.D18);
            _joystick = new GroveJoystick(_hat.Adc, GroveAdc.A0, GroveAdc.A2);
            
            // Setup button events
            _button.Pressed += OnButtonPressed;
            _button. Released += OnButtonReleased;
            
            _logger.LogInformation("Advanced Grove Example initialized");
        }
        
        private void OnButtonPressed(object?  sender, EventArgs e)
        {
            _logger.LogInformation("Button pressed - Stopping LED animations");
            _cts.Cancel();
        }
        
        private void OnButtonReleased(object? sender, EventArgs e)
        {
            _logger.LogInformation("Button released");
        }
        
        public async Task RunAsync()
        {
            // Start background tasks
            var joystickTask = MonitorJoystickAsync(_cts.Token);
            var led1Task = BlinkLedAsync(_led1, 500, _cts.Token);
            var led2Task = BlinkLedAsync(_led2, 750, _cts.Token);
            
            await Task.WhenAll(joystickTask, led1Task, led2Task);
        }
        
        private async Task MonitorJoystickAsync(CancellationToken cancellationToken)
        {
            var lastDirection = JoystickDirection. Center;
            
            while (!cancellationToken. IsCancellationRequested)
            {
                var direction = _joystick.GetDirection(deadzone: 0.2);
                
                if (direction != lastDirection)
                {
                    var (x, y) = _joystick.ReadNormalized();
                    _logger.LogInformation(
                        $"Joystick moved: {direction} (X={x:F2}, Y={y:F2})");
                    lastDirection = direction;
                }
                
                await Task.Delay(50, cancellationToken);
            }
        }
        
        private async Task BlinkLedAsync(GroveLed led, int intervalMs, CancellationToken cancellationToken)
        {
            while (!cancellationToken. IsCancellationRequested)
            {
                led.Toggle();
                await Task. Delay(intervalMs, cancellationToken);
            }
            
            led.Off();
        }
        
        public void Dispose()
        {
            _cts?. Cancel();
            _button. Pressed -= OnButtonPressed;
            _button.Released -= OnButtonReleased;
            
            _led1?. Dispose();
            _led2?.Dispose();
            _button?.Dispose();
            _joystick?.Dispose();
            _hat?.Dispose();
            _cts?.Dispose();
        }
    }
}
