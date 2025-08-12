namespace Caxivitual.Lunacub.Examples.MergeDependency;

public record SimpleResource(int Value);

public sealed class SimpleResourceDTO {
    public int Value { get; set; }
}