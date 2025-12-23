// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using System;
using System.Threading;
using GroveHatExample;
using GroveHatExample.Devices;
using Microsoft.Extensions.Logging;

using GroveDev;

namespace GroveHatExample
{
    class Program
    {
        private static ILogger<Program>?  _logger;
        
        static void Main(string[] args)
        {
            // Setup logging
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole().SetMinimumLevel(LogLevel.Information);
            });
            _logger = loggerFactory.CreateLogger<Program>();
            
            _logger. LogInformation("Starting Grove Base HAT Example");
            
            try
            {
                RunGroveExample();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in Grove application");
            }
        }
        
        static void RunGroveExample()
        {
            using var hat = new GroveBaseHat();
            
            // Setup LED on D5 port
            using var led = new GroveLed(hat.Gpio, GroveBaseHat.D5);
            _logger?.LogInformation($"LED initialized on D{GroveBaseHat.D5}");
            
            // Setup Button on D16 port
            using var button = new GroveButton(hat.Gpio, GroveBaseHat.D16);
            _logger?.LogInformation($"Button initialized on D{GroveBaseHat.D16}");
            
            // Button event handlers
            button.Pressed += (sender, e) =>
            {
                _logger?.LogInformation("Button PRESSED!");
                led.On();
            };
            
            button.Released += (sender, e) =>
            {
                _logger?.LogInformation("Button RELEASED!");
                led.Off();
            };
            
            // Setup Joystick on A0 (X-axis) and A1 (Y-axis)
            using var joystick = new GroveJoystick(hat.Adc, GroveAdc.A2, GroveAdc.A3);
            _logger?.LogInformation("Joystick initialized on A0 (X) and A1 (Y)");
            
            _logger?.LogInformation("Press Ctrl+C to exit");
            _logger?.LogInformation("Move joystick or press button to interact");
            
            var lastDirection = JoystickDirection.Center;
            int loopCount = 0;
            
            while (true)
            {
                // Read Ads1115 voltage (for demonstration)
                var ain = loopCount % 8;
                var voltage = hat.Ads1115Wrapper.ReadVoltage(ain);
                Console.WriteLine($"ADS1115 Channel {ain} Voltage: {voltage.Volts:F5} V");
                // _logger?.LogInformation($"ADS111
                // Read joystick every 100ms
                var (x, y) = joystick.ReadNormalized();
                var direction = joystick.GetDirection();
                
                // Log only when direction changes
                if (direction != lastDirection)
                {
                    _logger?.LogInformation(
                        $"Joystick: Direction={direction}, X={x:F2}, Y={y:F2}");
                    lastDirection = direction;
                    
                    // Blink LED based on direction
                    if (direction != JoystickDirection.Center)
                    {
                        led.Toggle();
                    }
                }
                
                // Periodically show raw ADC values
                if (loopCount % 50 == 0) // Every 5 seconds
                {
                    int xRaw = joystick.ReadXRaw();
                    int yRaw = joystick.ReadYRaw();
                    double xVolt = joystick.ReadXVoltage();
                    double yVolt = joystick.ReadYVoltage();
                    
                    _logger?.LogInformation(
                        $"ADC Values - X:  {xRaw} ({xVolt:F3}V), Y: {yRaw} ({yVolt:F3}V)");
                }
                
                loopCount++;
                Thread.Sleep(100);
            }
        }
    }
}

