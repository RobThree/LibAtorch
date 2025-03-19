namespace LibAtorch.Responses;

internal record OkResponse(byte[] RawData) : CommandResponse(RawData) { }
