using Ble.Interfaces;
using wclBluetooth;

namespace Ble.Windows;

public class WclGattService : IGattService
{
  private readonly wclGattClient _client;
  private readonly wclGattService _service;

  public WclGattService(wclGattClient client, wclGattService service)
  {
    _client = client;
    _service = service;
  }

  public Task<T> GetAsync<T>(string prop)
  {
    throw new NotImplementedException();
  }

  public Task<IGattCharacteristic> GetCharacteristicAsync(string characteristicUUID)
  {
    int res = _client.FindCharacteristic(_service, Wcl.Uuid.FromString(characteristicUUID), out wclGattCharacteristic? charstic);
    bool ok = res == wclCommon.wclErrors.WCL_E_SUCCESS && charstic != null;

    return Task.FromResult(ok ? new WclGattCharacteristic(_client, charstic.Value) : (IGattCharacteristic)null);
  }

  public Task SetAsync(string prop, object val)
  {
    throw new NotImplementedException();
  }
}
