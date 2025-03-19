using SIO = System.IO.Ports;

namespace LibAtorch;

public enum Parity
{
    None,
    Odd,
    Even,
    Mark,
    Space
}

internal static class ParityExtensions
{
    public static SIO.Parity ToSystemParity(this Parity parity) => parity switch
    {
        Parity.None => SIO.Parity.None,
        Parity.Odd => SIO.Parity.Odd,
        Parity.Even => SIO.Parity.Even,
        Parity.Mark => SIO.Parity.Mark,
        Parity.Space => SIO.Parity.Space,
        _ => throw new ArgumentOutOfRangeException(nameof(parity), parity, null)
    };
}