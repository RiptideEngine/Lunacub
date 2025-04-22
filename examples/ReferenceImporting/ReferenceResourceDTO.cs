namespace ReferenceImporting;

public sealed class ReferenceResourceDTO : ContentRepresentation {
    public ResourceID Reference { get; set; }
    public int Value { get; set; }
}