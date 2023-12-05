using Ble.Interfaces;
using vestervang.DotNetBlueZ;

namespace Ble.Linux;

public class LinuxBleDevice : IBleDevice
{
  private readonly Device _device;
  public bool IsConnected { get; private set; }

  public LinuxBleDevice(Device device)
  {
    _device = device;
    _device.Connected += (sender, args) => { IsConnected = true; return Task.CompletedTask; };
    _device.Disconnected += (sender, args) => { IsConnected = false; return Task.CompletedTask; };
  }

  public async Task<IGattService> GetServiceAsync(string serviceUUID)
  {
    return new LinuxBleService(await _device.GetServiceAsync(serviceUUID));
  }

  public Task DisconnectAsync()
  {
    return _device.DisconnectAsync();
  }

  public Task ConnectAsync()
  {
    return _device.ConnectAsync();
  }

  public Task ConnectProfileAsync(string UUID)
  {
    return _device.ConnectProfileAsync(UUID);
  }

  public Task DisconnectProfileAsync(string UUID)
  {
    return _device.DisconnectProfileAsync(UUID);
  }

  public Task PairAsync()
  {
    return _device.PairAsync();
  }

  public Task CancelPairingAsync()
  {
    return _device.CancelPairingAsync();
  }

  public Task<T> GetAsync<T>(string prop)
  {
    return _device.GetAsync<T>(prop);
  }

  public Task SetAsync(string prop, object val)
  {
    return _device.SetAsync(prop, val);
  }
}