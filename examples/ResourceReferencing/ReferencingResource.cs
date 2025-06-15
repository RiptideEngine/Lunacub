namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

public sealed class ReferencingResource {
    public SimpleResource? Reference;
}

public sealed class ReferencingResourceDTO : ContentRepresentation {
    public ResourceID ReferenceId { get; set; }
}