namespace LibAtorch.Responses;

internal record IntegerResponse(byte[] RawData) : QueryResponse(RawData)
{
    public int Value => RawData[0] << 16 | RawData[1] << 8 | RawData[2];
}
