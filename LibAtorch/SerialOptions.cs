namespace LibAtorch;

public record SerialOptions
{
    public required string PortName { get; init; }
    public int BaudRate { get; init; } = DefaultBaudRate;
    public Parity Parity { get; init; } = DefaultParity;
    public int DataBits { get; init; } = DefaultDataBits;
    public StopBits StopBits { get; init; } = DefaultStopBits;
    public TimeSpan ReadTimeout { get; init; } = DefaultReadTimeout;
    public TimeSpan WriteTimeout { get; init; } = DefaultWriteTimeout;

    public int RetryCommandCount { get; init; } = DefaultRetryCommandCount;

    public const int DefaultBaudRate = 9600;
    public const Parity DefaultParity = Parity.None;
    public const int DefaultDataBits = 8;
    public const StopBits DefaultStopBits = StopBits.One;
    public static readonly TimeSpan DefaultReadTimeout = TimeSpan.FromSeconds(3);
    public static readonly TimeSpan DefaultWriteTimeout = TimeSpan.FromSeconds(3);
    public const int DefaultRetryCommandCount = 3;
}
