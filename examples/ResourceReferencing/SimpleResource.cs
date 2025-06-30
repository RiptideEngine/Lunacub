namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

public record SimpleResource(int Value);

public sealed class SimpleResourceDTO : ContentRepresentation {
    public int Value { get; set; }
}