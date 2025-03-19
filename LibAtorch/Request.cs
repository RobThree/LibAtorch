using LibAtorch.Exceptions;

namespace LibAtorch;

internal record Request
{
    private static readonly byte[] _zerodata = [0x00, 0x00];
    private readonly byte[] _data;

    public byte ExpectedResponseLength { get; }
    public byte Type { get; }

    private Request(byte type, byte[] data, byte expectedresponselength)
    {
        Type = type;
        _data = data;
        ExpectedResponseLength = expectedresponselength;
    }

    internal Request(CommandType commandType, byte[] data)
        : this(
            Enum.IsDefined(commandType) ? (byte)commandType : throw new InvalidCommandTypeException((byte)commandType),
            data.Length == 2 ? data : throw new InvalidOperationException($"Invalid data length ({data.Length})"),
            1
        )
    { }

    internal Request(QueryType queryType)
        : this(
             Enum.IsDefined(queryType) ? (byte)queryType : throw new InvalidQueryTypeException((byte)queryType),
            _zerodata,
            7
        )
    { }

    internal Request(CommandType commandType, double value)
        : this(commandType, DoubleToByteArray(value)) { }

    internal Request(CommandType commandType, TimeSpan value)
        : this(commandType, TimespanToByteArray(value)) { }

    public byte[] ToFrame()
        => [0xB1, 0xB2, Type, _data[0], _data[1], 0xB6];

    private static byte[] DoubleToByteArray(double value)
    {
        var integerPart = (int)value;
        var fractionalPart = (int)((value - integerPart) * 100);
        return [(byte)integerPart, (byte)fractionalPart];
    }

    private static byte[] TimespanToByteArray(TimeSpan value)
    {
        var val = (ushort)value.TotalSeconds;
        return [(byte)((val >> 8) & 0xFF), (byte)(val & 0xFF)];
    }
}