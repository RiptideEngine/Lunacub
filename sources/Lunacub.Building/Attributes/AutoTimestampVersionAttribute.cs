namespace Caxivitual.Lunacub.Building.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[ExcludeFromCodeCoverage]
public class AutoTimestampVersionAttribute : Attribute {
    public string Format { get; }

    public AutoTimestampVersionAttribute([StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string format) {
        Format = format;
    }
}