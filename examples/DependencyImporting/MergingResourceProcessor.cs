using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Examples.DependencyImporting;

public sealed class MergingResourceProcessor : Processor<MergingResourceDTO, ProcessedMergingResourceDTO> {
    protected override ProcessedMergingResourceDTO Process(MergingResourceDTO input, ProcessingContext context) {
        List<int> values = [];
        
        context.Logger.LogInformation("Context Dependency Keys: {deps}", string.Join(", ", context.Dependencies.Keys));

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