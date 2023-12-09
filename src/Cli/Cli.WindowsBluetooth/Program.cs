using Ble.Interfaces;
using Ble.Windows;
using Cli.Common;
using Garmin.Ble.Lib;
using Garmin.Ble.Lib.Devices.Garmin;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

string deviceMacAddr = "90:F1:57:93:83:85";
string addr = deviceMacAddr.Replace(":", "");

var logger = new ConsoleLogger();
AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => Console.Error.WriteLine(eventArgs.ExceptionObject);

bool isWindows = OperatingSystem.IsWindows();

IBleDevice? bleDevice = await CreateWindowsBleDevice(addr);
if (bleDevice == null) { return; }

while (!bleDevice.IsConnected)
{
  await Task.Delay(1000);
}

var watch = new GarminDevice(logger, GarminDeviceConfig.Forerunner945LTE);
await watch.Init(bleDevice);

await TaskUtil.BlockForever();

async Task<IBleDevice?> CreateWindowsBleDevice(string macAddr)
{
  BluetoothAdapter adapter = await BluetoothAdapter.GetDefaultAsync();
  Windows.Devices.Radios.Radio radio = await adapter.GetRadioAsync();
  await radio.SetStateAsync(Windows.Devices.Radios.RadioState.On);

  var bleWatcher = new BluetoothLEAdvertisementWatcher
  {
    ScanningMode = BluetoothLEScanningMode.Active,
  };

  bleWatcher.AdvertisementFilter.BytePatterns.Add(new BluetoothLEAdvertisementBytePattern 
  { 
    Data = BinUtils.HexStringToByteArray(addr).AsBuffer(), 
  });

  bleWatcher.Received += async (watcher, args) =>
  {
    Console.WriteLine($"Found device {args.BluetoothAddress}");
    await Task.CompletedTask;
  };
  bleWatcher.Start();

  ulong.TryParse(macAddr, System.Globalization.NumberStyles.HexNumber, null, out var parsedId);
  BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(parsedId);

  Windows.Devices.Enumeration.DeviceAccessStatus accessStatus = await device.RequestAccessAsync();
  if (accessStatus != Windows.Devices.Enumeration.DeviceAccessStatus.Allowed) { return null; }

  //device.ConnectionStatusChanged += async (dev, o) =>
  //{
  //  if (dev.ConnectionStatus != BluetoothConnectionStatus.Connected) { return; }

  //  bleDevice = new WindowsBleDevice(device);
  //  var swim2 = new Swim2Device(logger);
  //  await swim2.Init(bleDevice);
  //};

  var servicesResult = await device.GetGattServicesAsync();
  foreach (var s in servicesResult.Services)
  {
    Console.WriteLine(s.Uuid);
    var charResult = await s.GetCharacteristicsAsync();
    await s.OpenAsync(GattSharingMode.Exclusive);
    //await s.GetIncludedServicesAsync();

    foreach (var c in charResult.Characteristics)
    {
      Console.WriteLine($"  {c.Uuid} ({c.AttributeHandle})");
    }
  }

  return new WindowsBleDevice(device);
}