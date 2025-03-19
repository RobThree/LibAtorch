namespace LibAtorch;

internal static class ByteArrayExtensionMethods
{
    public static string ToHex(this byte[] bytes) => string.Join(" ", bytes.Select(b => b.ToString("x2")));
}