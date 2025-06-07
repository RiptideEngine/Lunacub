namespace Caxivitual.Lunacub.Building.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class VersionTimestampAttribute : Attribute {
    public string Timestamp { get; }

    public VersionTimestampAttribute(string timestamp) {
        Timestamp = timestamp;
    }
}