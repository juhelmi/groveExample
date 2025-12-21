# Raspberry Pi Seeed Studio Grove HAT with C# .NET 9.0

Here's a comprehensive guide for using the Grove Base HAT for Raspberry Pi with C# .NET 9.0, including examples for LED, Button, ADC, and Grove Joystick.

---

## 1. Grove Base HAT Overview

The Seeed Studio Grove Base HAT provides: 
- **12 Digital Ports**: D5, D16, D18, D22, D24, D26 (configurable as GPIO)
- **6 Analog Ports**: A0, A2, A4, A6 (using onboard ADC - STM32)
- **4 PWM Ports**: PWM (software PWM on GPIO12, GPIO13, PWM, PWM1)
- **1 I2C Port**: For connecting I2C devices
- **Built-in ADC**: 12-bit ADC via I2C (address 0x04)

---

## 2. Required NuGet Packages

```bash
dotnet add package System.Device. Gpio
dotnet add package Iot.Device.Bindings
```

---

## 3. Grove HAT Library Classes

### GroveBaseHat. cs - Main HAT Interface

```csharp
using System;
using System.Device.Gpio;
using System. Device.I2c;

namespace GroveHatExample
{
    public class GroveBaseHat :  IDisposable
    {
        private readonly GpioController _gpio;
        private readonly GroveAdc _adc;
        
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
            _adc = new GroveAdc();
        }
        
        public GpioController Gpio => _gpio;
        public GroveAdc Adc => _adc;
        
        public void Dispose()
        {
            _gpio?.Dispose();
            _adc?. Dispose();
        }
    }
}
```

### GroveAdc.cs - ADC Interface

```csharp
using System;
using System.Device.I2c;

namespace GroveHatExample
{
    public class GroveAdc : IDisposable
    {
        private readonly I2cDevice _i2cDevice;
        private const byte ADC_ADDRESS = 0x04;
        
        // ADC channel mappings
        public const int A0 = 0;
        public const int A2 = 2;
        public const int A4 = 4;
        public const int A6 = 6;
        
        public GroveAdc()
        {
            var settings = new I2cConnectionSettings(1, ADC_ADDRESS);
            _i2cDevice = I2cDevice.Create(settings);
        }
        
        public int ReadValue(int channel)
        {
            if (channel < 0 || channel > 7)
                throw new ArgumentException("Channel must be between 0 and 7");
            
            byte[] writeBuffer = new byte[] { (byte)channel };
            byte[] readBuffer = new byte[2];
            
            _i2cDevice.Write(writeBuffer);
            System.Threading.Thread.Sleep(1); // Small delay for ADC conversion
            _i2cDevice.Read(readBuffer);
            
            // Combine bytes (12-bit ADC value)
            int value = (readBuffer[0] << 8) | readBuffer[1];
            return value & 0x0FFF; // Mask to 12 bits
        }
        
        public double ReadVoltage(int channel, double referenceVoltage = 3.3)
        {
            int rawValue = ReadValue(channel);
            return (rawValue / 4095.0) * referenceVoltage;
        }
        
        public void Dispose()
        {
            _i2cDevice?.Dispose();
        }
    }
}
```

---

## 4. Grove LED Example

### GroveLed.cs

```csharp
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
```

---

## 5. Grove Button Example

### GroveButton.cs

```csharp
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
```

---

## 6. Grove Joystick Example

### GroveJoystick.cs

```csharp
using System;

namespace GroveHatExample. Devices
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
        
        public int ReadXRaw() => _adc.ReadValue(_channelX);
        public int ReadYRaw() => _adc.ReadValue(_channelY);
        
        public double ReadXVoltage() => _adc.ReadVoltage(_channelX);
        public double ReadYVoltage() => _adc.ReadVoltage(_channelY);
        
        // Returns normalized values (-1.0 to 1.0)
        public (double x, double y) ReadNormalized()
        {
            int xRaw = ReadXRaw();
            int yRaw = ReadYRaw();
            
            // Center position is around 2048 (middle of 0-4095 range)
            double x = (xRaw - 2048.0) / 2048.0;
            double y = (yRaw - 2048.0) / 2048.0;
            
            // Clamp to -1.0 to 1.0
            x = Math. Clamp(x, -1.0, 1.0);
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
                return y > 0 ?  JoystickDirection.Up :  JoystickDirection.Down;
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
```

---

## 7. Complete Working Example

### Program.cs

```csharp
using System;
using System.Threading;
using GroveHatExample;
using GroveHatExample.Devices;
using Microsoft.Extensions.Logging;

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
            
            // Setup Joystick on A0 (X-axis) and A2 (Y-axis)
            using var joystick = new GroveJoystick(hat.Adc, GroveAdc.A0, GroveAdc.A2);
            _logger?.LogInformation("Joystick initialized on A0 (X) and A2 (Y)");
            
            _logger?.LogInformation("Press Ctrl+C to exit");
            _logger?.LogInformation("Move joystick or press button to interact");
            
            var lastDirection = JoystickDirection.Center;
            int loopCount = 0;
            
            while (true)
            {
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
                    int yRaw = joystick. ReadYRaw();
                    double xVolt = joystick.ReadXVoltage();
                    double yVolt = joystick.ReadYVoltage();
                    
                    _logger?.LogInformation(
                        $"ADC Values - X:  {xRaw} ({xVolt:F2}V), Y: {yRaw} ({yVolt:F2}V)");
                }
                
                loopCount++;
                Thread.Sleep(100);
            }
        }
    }
}
```

---

## 8. Advanced Example: All Features Combined

### AdvancedGroveExample.cs

```csharp
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
```

---

## 9. Project Structure

```
GroveHatExample/
├── GroveHatExample. csproj
├── Program. cs
├── GroveBaseHat.cs
├── GroveAdc.cs
├── Devices/
│   ├── GroveLed.cs
│   ├── GroveButton.cs
│   └── GroveJoystick.cs
└── AdvancedGroveExample.cs
```

---

## 10. Complete . csproj File

```xml
<Project Sdk="Microsoft.NET. Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Device.Gpio" Version="3.2.0" />
    <PackageReference Include="Iot.Device.Bindings" Version="3.2.0" />
    <PackageReference Include="Microsoft.Extensions. Logging" Version="9.0.0" />
    <PackageReference Include="Microsoft. Extensions.Logging.Console" Version="9.0.0" />
  </ItemGroup>
</Project>
```

---

## 11. Hardware Setup

### Grove Base HAT Port Connections

```
LED #1          → D5 Port
LED #2          → D16 Port  
Button          → D18 Port
Joystick X-axis → A0 Port (Analog)
Joystick Y-axis → A2 Port (Analog)
```

### Grove Port Pinout
```
Digital Port (4 pins):
  Pin 1: NC (Not Connected)
  Pin 2: GPIO Signal
  Pin 3: VCC (3.3V or 5V)
  Pin 4: GND

Analog Port (4 pins):
  Pin 1: ADC Signal
  Pin 2: NC
  Pin 3: VCC (3.3V)
  Pin 4: GND
```

---

## 12.  Debugging on VS Code

### launch.json for Grove HAT

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name":  "Grove HAT - Debug",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net9.0/GroveHatExample.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "console": "integratedTerminal",
            "stopAtEntry": false,
            "env": {
                "DOTNET_RUNNING_ENVIRONMENT": "Development"
            }
        },
        {
            "name":  "Grove HAT - Debug with sudo",
            "type": "coreclr",
            "request":  "launch",
            "preLaunchTask": "build",
            "program": "/usr/bin/sudo",
            "args": [
                "dotnet",
                "${workspaceFolder}/bin/Debug/net9.0/GroveHatExample.dll"
            ],
            "cwd": "${workspaceFolder}",
            "console": "integratedTerminal",
            "stopAtEntry": false
        }
    ]
}
```

---

## 13. Testing Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Run with sudo if GPIO permissions needed
sudo dotnet run

# Check I2C devices (should see 0x04 for ADC)
sudo i2cdetect -y 1

# Test ADC manually
sudo i2cget -y 1 0x04 0x00 w

# Monitor GPIO
gpiomonitor gpiochip0 5 16 18
```

---

## 14. Troubleshooting Grove HAT

### Enable I2C
```bash
sudo raspi-config
# Navigate to:  Interface Options → I2C → Enable
sudo reboot
```

### Check I2C Connection
```bash
sudo apt install i2c-tools
sudo i2cdetect -y 1
```

Expected output should show `04` (ADC address):
```
     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f
00:          -- 04 -- -- -- -- -- -- -- -- -- -- -- 
```

### GPIO Permissions
```bash
sudo usermod -aG gpio,i2c $USER
sudo reboot
```

### Check Grove HAT Firmware
```bash
# Download Grove HAT tools
git clone https://github.com/Seeed-Studio/grove. py
cd grove.py
sudo pip3 install . 

# Test ADC
grove_adc_read 0
```

---

## 15. Additional Grove Sensors

### Temperature Sensor (Analog)

```csharp
public class GroveTemperatureSensor : IDisposable
{
    private readonly GroveAdc _adc;
    private readonly int _channel;
    private const double B = 4275.0; // B value of thermistor
    private const double R0 = 100000.0; // R0 = 100k
    
    public GroveTemperatureSensor(GroveAdc adc, int channel)
    {
        _adc = adc;
        _channel = channel;
    }
    
    public double ReadTemperatureCelsius()
    {
        int rawValue = _adc.ReadValue(_channel);
        double resistance = (4095.0 / rawValue - 1.0) * R0;
        double temperature = 1.0 / (Math.Log(resistance / R0) / B + 1 / 298.15) - 273.15;
        return temperature;
    }
    
    public double ReadTemperatureFahrenheit()
    {
        return ReadTemperatureCelsius() * 9.0 / 5.0 + 32.0;
    }
    
    public void Dispose() { }
}
```

### Light Sensor (Analog)

```csharp
public class GroveLightSensor : IDisposable
{
    private readonly GroveAdc _adc;
    private readonly int _channel;
    
    public GroveLightSensor(GroveAdc adc, int channel)
    {
        _adc = adc;
        _channel = channel;
    }
    
    public int ReadRawValue() => _adc.ReadValue(_channel);
    
    public double ReadVoltage() => _adc.ReadVoltage(_channel);
    
    // Returns light intensity (0-100%)
    public double ReadLightIntensity()
    {
        int rawValue = ReadRawValue();
        return (rawValue / 4095.0) * 100.0;
    }
    
    public void Dispose() { }
}
```

---

This comprehensive guide should help you work with the Seeed Studio Grove Base HAT on Raspberry Pi using C# . NET 9.0. Let me know if you need additional examples or clarifications! 



