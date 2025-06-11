namespace Caxivitual.Lunacub.Building.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class VersionAttribute : Attribute {
    public string Timestamp { get; }

    public VersionAttribute(string timestamp) {
        Timestamp = timestamp;
    }
}