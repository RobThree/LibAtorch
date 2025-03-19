namespace LibAtorch.Responses;

internal record TimespanResponse(byte[] RawData) : QueryResponse(RawData)
{
    public TimeSpan Value => new(RawData[0], RawData[1], RawData[2]);
}
