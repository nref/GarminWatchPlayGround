using Ble.Interfaces;
//using Ble.Linux;
using Ble.Windows;
using Garmin.Ble.Lib;
using Garmin.Ble.Lib.Devices.Garmin;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Media.Audio;
//using vestervang.DotNetBlueZ;

const string MyWatchMac = "90:F1:57:93:83:85";
var deviceAddress = MyWatchMac;
var timeout = TimeSpan.FromSeconds(15);
var logger = new ConsoleLogger();
//logger.DisableDebugInZone(DebugZones.PackageSend);
//logger.DisableDebugInZone(DebugZones.BytesBleSend);
//logger.DisableDebugInZone(DebugZones.Package);
//logger.DisableDebugInZone(DebugZones.Protobuf);
//logger.DisableDebugInZone(DebugZones.BytesBle);
//logger.DisableDebugInZone(DebugZones.MessageSend);
//logger.DisableDebugInZone(DebugZones.MessageSendBytes);
//logger.DisableDebugInZone(DebugZones.FileTransferProgress);

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => Console.Error.WriteLine(eventArgs.ExceptionObject);

bool isLinux = OperatingSystem.IsLinux();
bool isWindows = OperatingSystem.IsWindows();

IBleDevice? bleDevice = null;
if (isLinux)
{
  //var adapters = await BlueZManager.GetAdaptersAsync();
  //if (adapters.Count == 0)
  //{
  //  throw new Exception("No Bluetooth adapters found.");
  //}

  //var adapter = adapters.First();

  //if (!await adapter.GetPoweredAsync())
  //  await adapter.SetPoweredAsync(true);

  //var device = await adapter.GetDeviceAsync(deviceAddress);
  //if (device == null)
  //{
  //  Console.WriteLine(
  //      $"Bluetooth peripheral with address '{deviceAddress}' not found. Use `bluetoothctl` or Bluetooth Manager to scan and possibly pair first.");
  //  return;
  //}

  //Console.WriteLine("Connecting...");
  //await device.ConnectAsync();
  //await device.WaitForPropertyValueAsync("Connected", value: true, timeout);
  //Console.WriteLine($"Connected to {await device.GetNameAsync()}.");

  //await device.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);

  //bleDevice = new LinuxBleDevice(device);
}
else if (isWindows)
{
// We have a problem on Windows because of CAR (Central Address Resolution).
// The Garmin watch asks for it but Windows doesn't have it and returns an error.
// The watch ignores us thereafter.
// Looks like a Nordic problem:
//
// ```
//  the windows 10 driver issues a Read by Type Request for characteristics within the handle range of 7 to 9.
//  The NRF52 responds with handles 7 & 8, but handle 9 characteristic (0x2aa6) is read as an "Unknown UUID [0x2aa6]."
//  Windows 10 then attempts a Read by Type Request on a characteristic with handle range 9 to 9 (the CAR).
//  The NRF52 returns with an error response "Error Code: Attribute not found."
//  After which no more attribute protocol requests are sent across.
// ```
// Source: https://devzone.nordicsemi.com/f/nordic-q-a/38478/unnecessary-central-address-resolution-characteristic-as-part-of-the-generic-access-service-breaking-hid-over-gatt-in-windows-10
//
// Looking at a packet trace in Wireshark, in fact it is the Garmin watch that sends the request and Windows that responds with an error.
//
// Unfortunately we cannot setup our own CAR in Windows:
//
// ```
// The following Services are reserved by the system and cannot be published at this time:
// 
//     Device Information Service (DIS)
//     Generic Attribute Profile Service (GATT)
//     Generic Access Profile Service (GAP)
//     Human Interface Device Service (HOGP)
//     Scan Parameters Service (SCP)
// 
//     Attempting to create a blocked service will result in BluetoothError.DisabledByPolicy being returned from the call to CreateAsync.
// ```
// Source: https://learn.microsoft.com/en-us/windows/uwp/devices-sensors/gatt-server

createDevice:
  {
    BluetoothAdapter adapter = await BluetoothAdapter.GetDefaultAsync();
    Windows.Devices.Radios.Radio radio = await adapter.GetRadioAsync();
    await radio.SetStateAsync(Windows.Devices.Radios.RadioState.On);

    string addr = deviceAddress.Replace(":", "").ToLower();
    ulong.TryParse(addr, System.Globalization.NumberStyles.HexNumber, null, out var parsedId);
    BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(parsedId);

    Windows.Devices.Enumeration.DeviceAccessStatus accessStatus = await device.RequestAccessAsync();
    if (accessStatus != Windows.Devices.Enumeration.DeviceAccessStatus.Allowed) { return; }

    //device.ConnectionStatusChanged += async (dev, o) =>
    //{
    //  if (dev.ConnectionStatus != BluetoothConnectionStatus.Connected) { return; }

    //  bleDevice = new WindowsBleDevice(device);
    //  var swim2 = new Swim2Device(logger);
    //  await swim2.Init(bleDevice);
    //};

    //var servicesResult = await device.GetGattServicesAsync();
    //foreach (var s in servicesResult.Services)
    //{
    //  Console.WriteLine(s.Uuid);
    //  var charResult = await s.GetCharacteristicsAsync();
    //  //await s.OpenAsync(GattSharingMode.Exclusive);
    //  //await s.GetIncludedServicesAsync();

    //  foreach (var c in charResult.Characteristics)
    //  {
    //    Console.WriteLine($"  {c.Uuid} ({c.AttributeHandle})");
    //  }
    //}

    bleDevice = new WindowsBleDevice(device);
  }
}
else
{
  return;
}

var swim2 = new Swim2Device(logger);
await swim2.Init(bleDevice);
//_ = Task.Run(swim2.DownloadAGpsData);


//Console.WriteLine("Press Enter to read file list:");
//Console.ReadLine();
// await swim2.DownloadGarminXml();
// await swim2.DownloadFile(5);

//swim2.DownloadAllActivities();

while (true)
{
  await Task.Delay(1000);
}