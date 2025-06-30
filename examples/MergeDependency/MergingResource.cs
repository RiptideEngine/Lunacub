namespace Caxivitual.Lunacub.Examples.MergeDependency;

public record MergingResource(int[] Values);

public sealed class MergingResourceDTO : ContentRepresentation {
    public ResourceID[] Dependencies { get; set; } = [];
}

public sealed class ProcessedMergingResourceDTO : ContentRepresentation {
    public int[] Values { get; set; } = [];
}