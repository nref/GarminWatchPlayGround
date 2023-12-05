using Ble.Interfaces;
//using Ble.Linux;
using Ble.Windows;
using Garmin.Ble.Lib;
using Garmin.Ble.Lib.Devices.Garmin;
using InTheHand.Bluetooth;
//using vestervang.DotNetBlueZ;

const string MyWatchMac = "90:F1:57:93:83:85";
var deviceAddress = MyWatchMac;
var timeout = TimeSpan.FromSeconds(15);

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
//else if (isWindows)
{
  //var scan = await Bluetooth.RequestLEScanAsync();

  //var filter = new BluetoothLEScanFilter();
  //filter.Services.Add(GarminConstants.UUID_SERVICE_GARMIN_3);
  //RequestDeviceOptions options = new()
  //{
    ////AcceptAllDevices = true,
  //};
  //options.Filters.Add(filter);
  //BluetoothDevice device = await Bluetooth.RequestDeviceAsync(options);
  var devices = await Bluetooth.GetPairedDevicesAsync();
  var device = devices.FirstOrDefault(d => d.Id == deviceAddress);

  if (device == null) { return; }

  await device.Gatt.ConnectAsync();

  if (!device.Gatt.IsConnected) { return; }
  device.GattServerDisconnected += HandleDisconnected;

  bleDevice = new WindowsBleDevice(device);
  List<GattService> services = await device.Gatt.GetPrimaryServicesAsync();
}
//else
//{
//  return;
//}

async void HandleDisconnected(object? sender, EventArgs e)
{
  if (sender is not BluetoothDevice bd) { return; }
  await bd.Gatt.ConnectAsync();
}

var logger = new ConsoleLogger();
logger.DisableDebugInZone(DebugZones.PackageSend);
logger.DisableDebugInZone(DebugZones.BytesBleSend);
logger.DisableDebugInZone(DebugZones.Package);
logger.DisableDebugInZone(DebugZones.Protobuf);
logger.DisableDebugInZone(DebugZones.BytesBle);
logger.DisableDebugInZone(DebugZones.MessageSend);
logger.DisableDebugInZone(DebugZones.MessageSendBytes);
logger.DisableDebugInZone(DebugZones.FileTransferProgress);
var swim2 = new Swim2Device(logger);
_ = Task.Run(swim2.DownloadAGpsData);

await swim2.Init(bleDevice);

//Console.WriteLine("Press Enter to read file list:");
//Console.ReadLine();
// await swim2.DownloadGarminXml();
// await swim2.DownloadFile(5);

 //swim2.DownloadAllActivities();

while (true)
    await Task.Delay(1000);