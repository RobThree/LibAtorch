namespace LibAtorch.Responses;

internal record BoolResponse(byte[] RawData) : QueryResponse(RawData)
{
    public bool Value => RawData[2] != 0x00;
}
