using Ble.Interfaces;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Ble.Windows;

public class WindowsBleService : IGattService
{
  private readonly GattDeviceService _gatt;

  public WindowsBleService(GattDeviceService gatt)
  {
    _gatt = gatt;
  }

  public Task<T> GetAsync<T>(string prop)
  {
    throw new NotImplementedException();
  }

  public async Task<IGattCharacteristic> GetCharacteristicAsync(string characteristicUUID)
  {
    GattCharacteristicsResult? result = await _gatt.GetCharacteristicsForUuidAsync(Guid.Parse(characteristicUUID));
    GattCharacteristic? characteristic = result?.Characteristics?.FirstOrDefault();

    if (result is null || characteristic is null || result.Status != GattCommunicationStatus.Success) 
    { 
      throw new Exception("Unknown characteristic"); 
    }

    return new WindowsGattCharacteristic(characteristic);
  }

  public Task SetAsync(string prop, object val)
  {
    throw new NotImplementedException();
  }
}
