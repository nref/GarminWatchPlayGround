namespace Cli.Common;

public static class TaskUtil
{
  public static async Task BlockForever(CancellationToken ct = default)
  {
    while (!ct.IsCancellationRequested)
    {
      await Task.Delay(1000, ct);
    }
  }
}
