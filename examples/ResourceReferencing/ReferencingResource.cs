namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

public sealed class ReferencingResource {
    public SimpleResource? Reference;
}

public sealed class ReferencingResourceDTO {
    public ResourceAddress Reference { get; set; }
}