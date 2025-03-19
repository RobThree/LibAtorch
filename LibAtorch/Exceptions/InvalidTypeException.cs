namespace LibAtorch.Exceptions;

public abstract class InvalidTypeException<T>(string kind, T type)
    : LibAtorchException($"Invalid {kind} type '{type}'")
{
    public string Kind { get; } = kind;
    public T Type { get; } = type;
}
