using Caxivitual.Lunacub.Building.Attributes;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

[AutoTimestampVersion("yyyMMddHHmmss")]
public sealed partial class EmittableResourceProcessor : Processor<EmittableResourceDTO, ProcessedEmittableResourceDTO> {
    protected override ProcessedEmittableResourceDTO Process(EmittableResourceDTO importedObject, ProcessingContext context) {
        context.ProceduralResources.Add(new() {
            Object = new SimpleResourceDTO {
                Value = importedObject.Value,
            },
        }, out var address);
        
        return new(importedObject.Value, address);
    }
}