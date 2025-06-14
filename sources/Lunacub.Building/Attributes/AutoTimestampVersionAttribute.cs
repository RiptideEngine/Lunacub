namespace Caxivitual.Lunacub.Building.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class AutoTimestampVersionAttribute : Attribute {
    public string Format { get; }

    public AutoTimestampVersionAttribute(string format) {
        Format = format;
    }
}