using Ble.Interfaces;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;

namespace Ble.Windows;

public class WindowsGattCharacteristic : IGattCharacteristic
{
  private readonly GattCharacteristic _gattChar;

  public event Interfaces.GattCharacteristicEventHandlerAsync? Value;

  public WindowsGattCharacteristic(GattCharacteristic gattChar)
  {
    _gattChar = gattChar;
    _gattChar.ValueChanged += HandleValueChanged;
  }

  private async void HandleValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
  {
    await OnValueIn(args);
  }

  public Task<string> GetUUIDAsync()
  {
    return Task.FromResult(_gattChar.Uuid.ToString());
  }

  public async Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
  {
    var ibuf = (await _gattChar.ReadValueAsync()).Value;
    CryptographicBuffer.CopyToByteArray(ibuf, out byte[] buffer);
    return buffer;
  }

  public async Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
  {
    bool withResponse = Options.TryGetValue("type", out var value) && value is string type && type == "request";
    if (withResponse) 
    {
       var response = await _gattChar.WriteValueWithResultAsync(Value.AsBuffer(), GattWriteOption.WriteWithResponse);
    }
    else
    {
      await _gattChar.WriteValueAsync(Value.AsBuffer(), GattWriteOption.WriteWithoutResponse);
    }
  }

  private Task OnValueIn(GattValueChangedEventArgs e) => Value?.Invoke(this, new WindowsGattCharacteristicArgs(e)) ?? Task.CompletedTask;

}
