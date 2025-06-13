namespace Caxivitual.Lunacub.Examples.DependencyImporting;

public sealed partial class MergingResourceProcessor : Processor<MergingResourceDTO, ProcessedMergingResourceDTO> {
    protected override ProcessedMergingResourceDTO Process(MergingResourceDTO input, ProcessingContext context) {
        List<int> values = [];
        
        foreach (var dependencyId in input.Dependencies) {
            if (!context.Dependencies.TryGetValue(dependencyId, out var dependency)) continue;
            if (dependency is not SimpleResourceDTO dependencyResource) continue;
            
            values.Add(dependencyResource.Value);
        }

        return new() {
            Values = values.ToArray(),
        };
    }
}