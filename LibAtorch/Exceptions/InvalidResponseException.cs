namespace LibAtorch.Exceptions;

public class InvalidResponseException(byte type, byte[] response)
    : LibAtorchException("Invalid response")
{
    public byte Type { get; } = type;
    public byte[] Response { get; } = response;

    public string HexResponse => Response.ToHex();
}
