namespace Caxivitual.Lunacub.Examples.MergeDependency;

public record MergingResource(int[] Values);

public sealed class MergingResourceDTO {
    public ResourceAddress[] Dependencies { get; set; } = [];
}

public sealed class ProcessedMergingResourceDTO {
    public int[] Values { get; set; } = [];
}