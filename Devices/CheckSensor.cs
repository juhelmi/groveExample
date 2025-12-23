using System;
using System.Device.I2c;

namespace GroveDev
{

class TestPrint
{
    private int _i2cBusId = 1;
    private List<int> _addresses = new List<int> { 0x76, 0x77 };

    public TestPrint()
    : this(1, null)
    {        
    }

    public TestPrint(int i2cBusId, List<int>? addresses)
    {
        _i2cBusId = i2cBusId;
        if (addresses is not null)
        {
            _addresses = new List<int>(addresses);
        }
    }   

    public void PrintToConsole()
    {
        foreach (var address in _addresses)
        {
            CheckSensor((byte)address);
        }
    }

    public void CheckSensor(byte address)
    {
        try
        {
            var settings = new I2cConnectionSettings(_i2cBusId, address);
            using var device = I2cDevice.Create(settings);

            // BMP280 chip ID register = 0xD0
            Span<byte> writeBuffer = stackalloc byte[] { 0xD0 };
            Span<byte> readBuffer = stackalloc byte[1];

            device.WriteRead(writeBuffer, readBuffer);
            byte chipId = readBuffer[0];

            Console.WriteLine($"Address 0x{address:X2}: Chip ID = 0x{chipId:X2}");

            switch (chipId)
            {
                case 0x55:
                    Console.WriteLine(" → Detected BMP180");
                    break;

                case 0x58:
                case 0x60:
                    Console.WriteLine(" → Detected BMP280");
                    break;
                case 0x61:
                    Console.WriteLine(" → Detected BME680");
                    break;

                default:
                    Console.WriteLine(" → Unknown device");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Address 0x{address:X2}: No response ({ex.Message})");
        }
    }
}

}
