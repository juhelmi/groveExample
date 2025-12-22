# groveExample and demo SW
Csharp study for Gove HAT on Raspberry Pi

dotnet new console -n groveExample
cd groveExample

# used NuGet packages

dotnet add package System.Device.Gpio   
dotnet add package Iot.Device.Bindings   

dotnet add package Microsoft.Extensions.Logging   
dotnet add package Microsoft.Extensions.Logging.Console   

# Own HAT ADC routine 
There is GroveDev namespace and python to C# converted ADC class named GroveADC. My HAS have it at address 0x8, but 0x4 looks to be possible also.

GroveAdc.ReadVoltage returns value in mV unit.







