using SIO = System.IO.Ports;

namespace LibAtorch;

public enum StopBits
{
    None,
    One,
    Two,
    OnePointFive
}

internal static class StopBitsExtensions
{
    public static SIO.StopBits ToSystemStopBits(this StopBits stopBits) => stopBits switch
    {
        StopBits.None => SIO.StopBits.None,
        StopBits.One => SIO.StopBits.One,
        StopBits.Two => SIO.StopBits.Two,
        StopBits.OnePointFive => SIO.StopBits.OnePointFive,
        _ => throw new ArgumentOutOfRangeException(nameof(stopBits), stopBits, null)
    };
}