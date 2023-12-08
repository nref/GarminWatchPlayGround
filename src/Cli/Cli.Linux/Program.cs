using Ble.Interfaces;
using Ble.Linux;
using Cli.Common;
using Garmin.Ble.Lib;
using Garmin.Ble.Lib.Devices.Garmin;
using vestervang.DotNetBlueZ;

string deviceMacAddr = "90:F1:57:93:83:85";
string addr = deviceMacAddr.Replace(":", "");

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


var adapters = await BlueZManager.GetAdaptersAsync();
if (adapters.Count == 0)
{
  throw new Exception("No Bluetooth adapters found.");
}

var adapter = adapters.First();

if (!await adapter.GetPoweredAsync())
  await adapter.SetPoweredAsync(true);

var device = await adapter.GetDeviceAsync(deviceMacAddr);
if (device == null)
{
  Console.WriteLine(
      $"Bluetooth peripheral with address '{deviceMacAddr}' not found. Use `bluetoothctl` or Bluetooth Manager to scan and possibly pair first.");
  return;
}

var timeout = TimeSpan.FromSeconds(15);
Console.WriteLine("Connecting...");
await device.ConnectAsync();
await device.WaitForPropertyValueAsync("Connected", value: true, timeout);
Console.WriteLine($"Connected to {await device.GetNameAsync()}.");

await device.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);

IBleDevice? bleDevice = new LinuxBleDevice(device);

if (bleDevice == null) { return; }

while (!bleDevice.IsConnected)
{
  await Task.Delay(1000);
}

var swim2 = new Swim2Device(logger);
await swim2.Init(bleDevice);
//_ = Task.Run(swim2.DownloadAGpsData);

//Console.WriteLine("Press Enter to read file list:");
//Console.ReadLine();
// await swim2.DownloadGarminXml();
// await swim2.DownloadFile(5);

//swim2.DownloadAllActivities();

await TaskUtil.BlockForever();
