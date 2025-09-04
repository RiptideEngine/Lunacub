namespace Caxivitual.Lunacub.Compilation;

[ExcludeFromCodeCoverage]
public static class CompilingConstants {
    public const string CompiledResourceExtension = ".lcr";     // Lunacub Compiled Content

    public static ReadOnlySpan<byte> MagicIdentifier => "LCCR"u8;
    public static ReadOnlySpan<byte> ResourceDataChunkTag => "DATA"u8;
    public static ReadOnlySpan<byte> ImportOptionsChunkTag => "IOPT"u8;
    public static ReadOnlySpan<byte> DeserializationChunkTag => "DESR"u8;
}