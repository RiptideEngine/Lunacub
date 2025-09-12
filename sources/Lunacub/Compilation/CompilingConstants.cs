namespace Caxivitual.Lunacub.Compilation;

[ExcludeFromCodeCoverage]
public static class CompilingConstants {
    public const string CompiledResourceExtension = ".lcr";     // Lunacub Compiled Content

    public static Tag MagicIdentifier => new("LCCR"u8);
    public static Tag ResourceDataChunkTag => new("DATA"u8);
    public static Tag ImportOptionsChunkTag => new("IOPT"u8);
    public static Tag DeserializationChunkTag => new("DESR"u8);
}