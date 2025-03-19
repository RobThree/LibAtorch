namespace LibAtorch.Exceptions;

public class InvalidQueryTypeException(byte type)
    : InvalidTypeException<byte>("query", type)
{ }