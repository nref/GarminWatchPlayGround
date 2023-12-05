using Ble.Interfaces;

namespace Ble.Windows;

public class WclGattCharacteristicArgs : IGattCharacteristicValueEventArgs
{
  private readonly byte[] _args;

  public WclGattCharacteristicArgs(byte[] args)
  {
    _args = args;
  }

  public byte[] Value => _args;
}
