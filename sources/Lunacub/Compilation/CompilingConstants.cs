namespace Caxivitual.Lunacub.Compilation;

[ExcludeFromCodeCoverage]
public static class CompilingConstants {
    public const string CompiledResourceExtension = ".lcr";     // Lunacub Compiled Content
    public const string ReportExtension = ".lrer";              // Lunacub Content Export Report

    public static ReadOnlySpan<byte> MagicIdentifier => "LCCR"u8;
    public static ReadOnlySpan<byte> ResourceDataChunkTag => "DATA"u8;
    public static ReadOnlySpan<byte> ImportOptionsChunkTag => "IOPT"u8;
    public static ReadOnlySpan<byte> DeserializationChunkTag => "DESR"u8;
    public static ReadOnlySpan<byte> TagChunkTag => "TAGS"u8;
}