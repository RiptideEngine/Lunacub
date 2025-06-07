namespace Caxivitual.Lunacub.Examples.ReferenceImporting;

public class ReferenceResource {
    public ReferenceResource? Reference { get; set; }
    public int Value { get; set; }
}

public sealed class ReferenceResourceDTO : ContentRepresentation {
    public ResourceID Reference { get; set; }
    public int Value { get; set; }
}