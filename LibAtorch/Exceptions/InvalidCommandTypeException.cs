namespace LibAtorch.Exceptions;

public class InvalidCommandTypeException(byte type)
    : InvalidTypeException<byte>("command", type)
{ }
