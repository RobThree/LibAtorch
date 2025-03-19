namespace LibAtorch.Responses;

internal abstract record CommandResponse(byte[] RawData) : Response(RawData) { }
