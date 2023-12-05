using Ble.Interfaces;
using InTheHand.Bluetooth;
using Linux.Bluetooth;

namespace Ble.Windows;

public class WindowsBleDevice : IBleDevice
{
  private readonly BluetoothDevice _device;

  public WindowsBleDevice(BluetoothDevice device)
  {
    _device = device;
  }

  public Task CancelPairingAsync()
  {
    throw new NotImplementedException();
  }

  public async Task ConnectAsync()
  {
    await _device.Gatt.ConnectAsync();
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

  public async Task<IGattService> GetServiceAsync(string serviceUUID)
  {
    GattService service = await _device.Gatt.GetPrimaryServiceAsync(BluetoothUuid.FromGuid(Guid.Parse(serviceUUID)));
    return new WindowsBleService(service);
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

public class WindowsBleService : IGattService
{
  private readonly GattService _gatt;

  public WindowsBleService(GattService gatt)
  {
    _gatt = gatt;
  }

  public Task<T> GetAsync<T>(string prop)
  {
    throw new NotImplementedException();
  }

  public async Task<IGattCharacteristic> GetCharacteristicAsync(string characteristicUUID)
  {
    InTheHand.Bluetooth.GattCharacteristic gc = await _gatt.GetCharacteristicAsync(BluetoothUuid.FromGuid(Guid.Parse(characteristicUUID)));
    return new WindowsGattCharacteristic(gc);
  }

  public Task SetAsync(string prop, object val)
  {
    throw new NotImplementedException();
  }
}

public class WindowsGattCharacteristic : IGattCharacteristic
{
  private readonly InTheHand.Bluetooth.GattCharacteristic _gattChar;

  public event Interfaces.GattCharacteristicEventHandlerAsync Value;

  public WindowsGattCharacteristic(InTheHand.Bluetooth.GattCharacteristic gattChar)
  {
    _gattChar = gattChar;

    _gattChar.CharacteristicValueChanged += HandleCharacteristicValueChanged;
  }

  private async void HandleCharacteristicValueChanged(object? sender, GattCharacteristicValueChangedEventArgs e)
  {
    await OnValueIn(_gattChar, e);
  }

  public Task<string> GetUUIDAsync()
  {
    return Task.FromResult(_gattChar.Uuid.ToString());
  }

  public async Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
  {
    return await _gattChar.ReadValueAsync();
  }

  public async Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
  {
    bool withResponse = Options.TryGetValue("type", out var value) && value is string type && type == "request";
    if (withResponse) 
    {
      await _gattChar.WriteValueWithResponseAsync(Value);
    }
    else
    {
      await _gattChar.WriteValueWithoutResponseAsync(Value);
    }
  }

  private Task? OnValueIn(InTheHand.Bluetooth.GattCharacteristic sender, GattCharacteristicValueChangedEventArgs e)
  {
    return Value?.Invoke(this, new WindowsGattCharacteristicArgs(e));
  }

}
internal class WindowsGattCharacteristicArgs : IGattCharacteristicValueEventArgs
{
    private readonly GattCharacteristicValueChangedEventArgs _gattCharacteristicValueEventArgsImplementation;

    public WindowsGattCharacteristicArgs(GattCharacteristicValueChangedEventArgs gattCharacteristicValueEventArgs)
    {
        _gattCharacteristicValueEventArgsImplementation = gattCharacteristicValueEventArgs;
    }

    public byte[] Value => _gattCharacteristicValueEventArgsImplementation.Value;
}
