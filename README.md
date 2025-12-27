# groveExample and demo SW
## C# study for Grove HAT on Raspberry Pi

Drivers are tested without application here and later used in other project. Looks that there could be multiple implementations for these but searching requires work. Some parts of code has quite same functionality as is used in grove.py. Pydantic or type hinting has not been used for grove.py. Type hinting is nice for new writer. When code is converted to C#, this feature is fixed on same time. It is now available vith VS code and C#.

These drivers mostly use GPIO or I2C devices. Currently .NET 9 is used and later .NET 10. Target to run on Raspberry Pi Zero 2 W but development on Raspberry Pi 5.

# Some memo commands for project creation
Basic project creation:

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







