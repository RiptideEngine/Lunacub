namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

public class ReferenceResource {
    public ReferenceResource? Reference { get; set; }
    public int Value { get; set; }
}

public sealed class ReferenceResourceDTO : ContentRepresentation {
    public ResourceID Reference { get; set; }
    public int Value { get; set; }
}