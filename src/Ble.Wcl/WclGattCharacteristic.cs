using Ble.Interfaces;
using wclBluetooth;

namespace Ble.Windows;

public class WclGattCharacteristic : IGattCharacteristic
{
  public event GattCharacteristicEventHandlerAsync? Value;

  private readonly wclGattClient _client;
  private readonly wclGattCharacteristic _charstic;

  public WclGattCharacteristic(wclGattClient client, wclGattCharacteristic charstic)
  {
    _client = client;
    _charstic = charstic;

    _client.OnCharacteristicChanged += (object sender, ushort handle, byte[] value) =>
    {
      Value?.Invoke(this, new WclGattCharacteristicArgs(value));
    };

    _client.SubscribeForNotifications(charstic);
  }

  public Task<string> GetUUIDAsync()
  {
    return Task.FromResult(Wcl.Uuid.ToString(_charstic.Uuid));
  }

  public Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
  {
    int res = _client.ReadCharacteristicValue(_charstic, wclGattOperationFlag.goReadFromDevice, out byte[] value);
    bool ok = res == wclCommon.wclErrors.WCL_E_SUCCESS;

    return Task.FromResult(ok ? value : Array.Empty<byte>());
  }

  public Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
  {
    bool withResponse = Options.TryGetValue("type", out var value) && value is string type && type == "request";
    var writeKind = withResponse ? wclGattWriteKind.wkWithResponse: wclGattWriteKind.wkWithoutResponse;

    writeKind = wclGattWriteKind.wkWithResponse;
    int res =_client.WriteCharacteristicValue(_charstic, Value, WriteKind: writeKind);
    return Task.CompletedTask;
  }
}