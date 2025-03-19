namespace LibAtorch.Responses;

internal record ErrorResponse(byte[] RawData) : CommandResponse(RawData) { }
