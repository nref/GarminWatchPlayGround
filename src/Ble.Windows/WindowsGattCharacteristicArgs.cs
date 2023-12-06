using Ble.Interfaces;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Ble.Windows;

public class WindowsGattCharacteristicArgs : IGattCharacteristicValueEventArgs
{
    private readonly GattValueChangedEventArgs _args;

    public WindowsGattCharacteristicArgs(GattValueChangedEventArgs args)
    {
        _args = args;
    }

  public byte[] Value => _args.CharacteristicValue.ToArray();
}
