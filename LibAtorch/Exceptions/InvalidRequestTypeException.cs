namespace LibAtorch.Exceptions;

public class InvalidRequestTypeException(byte type)
    : InvalidTypeException<byte>("request", type)
{ }
