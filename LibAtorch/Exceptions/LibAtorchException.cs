namespace LibAtorch.Exceptions;

public abstract class LibAtorchException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{ }