using Ble.Interfaces;
using Ble.Windows;
using Cli.Common;
using Garmin.Ble.Lib;
using Garmin.Ble.Lib.Devices.Garmin;
using wclBluetooth;
using wclCommon;

string deviceMacAddr = "90:F1:57:93:83:85";
string addr = deviceMacAddr.Replace(":", "");

var logger = new ConsoleLogger();
AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => Console.Error.WriteLine(eventArgs.ExceptionObject);

new AntiNag().Run();

bool isWindows = OperatingSystem.IsWindows();

IBleDevice? bleDevice = await CreateWclBleDevice(addr);
if (bleDevice == null) { return; }

while (!bleDevice.IsConnected)
{
  await Task.Delay(1000);
}

var swim2 = new Swim2Device(logger);
await swim2.Init(bleDevice);

await TaskUtil.BlockForever();

static async Task<IBleDevice?> CreateWclBleDevice(string macAddr)
{
  int res = 0;
  bool ok = true;
  long addr = Convert.ToInt64(macAddr, 16);

  //var watcher = new wclBluetoothLeBeaconWatcher();
  //watcher.OnAdvertisementFrameInformation  += (object Sender, long Address, long Timestamp, sbyte Rssi,
  //          string Name, wclBluetoothLeAdvertisementType PacketType, wclBluetoothLeAdvertisementFlag Flags) =>
  //{
  //  if (Address != addr) { return; }
  //};

  // Required apps without a message pump e.g. console app
  // https://docs.btframework.com/wifi/.net/html/M_wclCommon_wclMessageBroadcaster_SetSyncMethod.htm
  // https://forum.btframework.com/index.php?topic=3520.msg9021#msg9021
  wclMessageBroadcaster.SetSyncMethod(wclMessageSynchronizationKind.skApc);

  var manager = new wclBluetoothManager();

  manager.OnNumericComparison += (object sender, wclBluetoothRadio radio, long addr, uint number, out bool confirm) => confirm = true;
  manager.OnPasskeyNotification += (o, r, a, p) => Console.WriteLine($"Passkey notification from {addr}");
  manager.OnPasskeyRequest += (object sender, wclBluetoothRadio radio, long addr, out uint passKey) =>
  {
    Console.WriteLine($"  Please enter the passkey for {addr}");
    string? line = Console.ReadLine();
    if (!uint.TryParse(line, out passKey))
    {
      passKey = 0;
    }
  };

  manager.OnPinRequest += (object sender, wclBluetoothRadio radio, long addr, out string pin) =>
  {
    Console.WriteLine($"  Please enter the PIN for {addr}");
    pin = Console.ReadLine() ?? "";
  };
  manager.OnAuthenticationCompleted += (o, r, a, e) => Console.WriteLine($"Successfully paired with {a}");
  manager.OnConfirm += (object sender, wclBluetoothRadio radio, long addr, out bool confirm) => confirm = true;
  manager.OnDiscoveringStarted += (sender, e) => Console.WriteLine("Discovery started");
  manager.OnDiscoveringCompleted += (sender, radio, error) => Console.WriteLine("Discovery completed");
  manager.OnDeviceFound += (sender, radio, addr) =>
  {
    Console.WriteLine($"Found new device {addr}");
  };

  var client = new wclGattClient();
  client.OnConnect += (o, e) =>
  {
    Console.WriteLine($"{(o as wclGattClient)!.Address} connected");
  };

  res = manager.Open();
  ok = res == wclCommon.wclErrors.WCL_E_SUCCESS;
  if (!ok) { return null; }

  res = manager.GetLeRadio(out wclBluetoothRadio? radio);
  ok = res == wclCommon.wclErrors.WCL_E_SUCCESS;
  if (!ok) { return null; }
  if (radio is null) { return null; }

  //res = watcher.Start(radio);
  //ok = res == wclCommon.wclErrors.WCL_E_SUCCESS;
  //if (!ok) { return null; }

  res = radio.EnumPairedDevices(wclBluetoothDiscoverKind.dkBle, out long[]? devices);
  ok = res == wclCommon.wclErrors.WCL_E_SUCCESS;
  if (!ok) { return null; }

  client.Address = devices.FirstOrDefault(d => d == addr);
  if (client.Address == 0) 
  {
    Console.WriteLine($"Device {addr:X8} not paired, searching for it");
    Console.WriteLine("  Please put the device in pairing mode");

    res = radio.RemotePair(addr, wclBluetoothPairingMethod.pmLe);
    ok = res == wclCommon.wclErrors.WCL_E_SUCCESS;
    if (!ok) { return null; }

    bool tmpPaired = false;
    while (!tmpPaired)
    {
      res = radio.GetRemotePaired(addr, out tmpPaired);
      if (!ok) { return null; }
      await Task.Delay(1000);
    }
  }

  Console.WriteLine($"Found device {client.Address:X8}");

  res = radio.GetRemoteName(client.Address, out string name);
  ok = res == wclCommon.wclErrors.WCL_E_SUCCESS;
  if (!ok) { return null; }
  Console.WriteLine($"  Name: {name}");

  res = client.Connect(radio);
  ok = res == wclCommon.wclErrors.WCL_E_SUCCESS;
  if (!ok) { return null; }

  return new WclBleDevice(radio, client);
}
