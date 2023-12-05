using wclBluetooth;

namespace Ble.Windows;

public static class Wcl
{
  public static class Uuid
  {
    public static wclGattUuid FromGuid(Guid uuid) => new()
    {
      LongUuid = uuid,
      IsShortUuid = false,
    };

    public static wclGattUuid FromString(string uuid) => FromGuid(Guid.Parse(uuid));
    public static string ToString(wclGattUuid uuid) => uuid.IsShortUuid ? $"{uuid.ShortUuid}" : $"{uuid.LongUuid}";
  }
}
