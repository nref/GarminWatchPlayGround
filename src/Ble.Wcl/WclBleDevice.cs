using Ble.Interfaces;
using wclBluetooth;

namespace Ble.Windows;

public class WclBleDevice : IBleDevice
{
  private readonly wclBluetoothRadio _radio;
  private readonly wclGattClient _client;
  public bool IsConnected { get;private set; } 

  public WclBleDevice(wclBluetoothRadio radio, wclGattClient client)
  {
    _radio = radio;
    _client = client;

    _client.OnConnect += (o, e) => IsConnected = true;
    _client.OnDisconnect += (o, e) => IsConnected = false;
  }

  public Task CancelPairingAsync()
  {
    throw new NotImplementedException();
  }

  public Task ConnectAsync()
  {
    _client.Connect(_radio);
    return Task.CompletedTask;
  }

  public Task ConnectProfileAsync(string UUID)
  {
    throw new NotImplementedException();
  }

  public Task DisconnectAsync()
  {
    _client.Disconnect();
    return Task.CompletedTask;
  }

  public Task DisconnectProfileAsync(string UUID)
  {
    throw new NotImplementedException();
  }

  public Task<T> GetAsync<T>(string prop)
  {
    throw new NotImplementedException();
  }

  public Task<IGattService?> GetServiceAsync(string serviceUUID)
  {
    wclGattUuid uuid = Wcl.Uuid.FromString(serviceUUID);
    int res = _client.FindService(uuid, out wclGattService? service);
    bool ok = res == wclCommon.wclErrors.WCL_E_SUCCESS && service != null;

    return Task.FromResult(ok ? new WclGattService(_client, service.Value) : (IGattService?)null);
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
