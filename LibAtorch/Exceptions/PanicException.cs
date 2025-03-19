namespace LibAtorch.Exceptions;

public class PanicException(string message, Exception? innerException = null)
    : LibAtorchException(message, innerException)
{ }