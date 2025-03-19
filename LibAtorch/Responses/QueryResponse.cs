namespace LibAtorch.Responses;

internal abstract record QueryResponse(byte[] RawData) : Response(RawData) { }
