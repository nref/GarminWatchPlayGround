using Ble.Interfaces;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Ble.Windows;

public class WindowsGattCharacteristic : IGattCharacteristic
{
  public event Interfaces.GattCharacteristicEventHandlerAsync? Value;

  private readonly GattCharacteristic _gattChar;

  public WindowsGattCharacteristic(GattCharacteristic gattChar)
  {
    _gattChar = gattChar;

    _ = Task.Run(async () =>
    {
      _gattChar.ValueChanged += HandleValueChanged;
      GattCommunicationStatus status = await _gattChar
        .WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
    });
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
    return ibuf.ToArray();
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
