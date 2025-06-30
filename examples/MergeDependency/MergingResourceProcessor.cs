using Caxivitual.Lunacub.Building.Attributes;

namespace Caxivitual.Lunacub.Examples.MergeDependency;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class MergingResourceProcessor : Processor<MergingResourceDTO, ProcessedMergingResourceDTO> {
    protected override ProcessedMergingResourceDTO Process(MergingResourceDTO importedObject, ProcessingContext context) {
        List<int> values = [];
        
        foreach (var dependencyId in importedObject.Dependencies) {
            if (!context.Dependencies.TryGetValue(dependencyId, out var dependency)) continue;
            if (dependency is not SimpleResourceDTO dependencyResource) continue;
            
            values.Add(dependencyResource.Value);
        }

        return new() {
            Values = values.ToArray(),
        };
    }
}