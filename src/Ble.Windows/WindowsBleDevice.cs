using Ble.Interfaces;
using Windows.Devices.Bluetooth;

namespace Ble.Windows;

public class WindowsBleDevice : IBleDevice
{
  private readonly BluetoothLEDevice _device;
  public bool IsConnected { get;private set; } 

  public WindowsBleDevice(BluetoothLEDevice device)
  {
    _device = device;
    _device.ConnectionStatusChanged += HandleConnectionStatusChanged;
  }

  private void HandleConnectionStatusChanged(BluetoothLEDevice sender, object args)
  {
    IsConnected = sender.ConnectionStatus == BluetoothConnectionStatus.Connected;
  }

  public Task CancelPairingAsync()
  {
    throw new NotImplementedException();
  }

  public Task ConnectAsync()
  {
    return Task.CompletedTask;
  }

  public Task ConnectProfileAsync(string UUID)
  {
    throw new NotImplementedException();
  }

  public Task DisconnectAsync()
  {
    throw new NotImplementedException();
  }

  public Task DisconnectProfileAsync(string UUID)
  {
    throw new NotImplementedException();
  }

  public Task<T> GetAsync<T>(string prop)
  {
    throw new NotImplementedException();
  }

  public Task<IGattService> GetServiceAsync(string serviceUUID)
  {
    var service = _device.GetGattService(Guid.Parse(serviceUUID));
    return Task.FromResult((IGattService)new WindowsBleService(service));
  }

  public Task PairAsync()
  {
    throw new NotImplementedException();
  }

  public Task SetAsync(string prop, object val)
  {
    throw new NotImplementedException();
  }
}