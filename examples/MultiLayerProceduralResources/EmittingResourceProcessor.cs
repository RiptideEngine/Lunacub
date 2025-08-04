using Caxivitual.Lunacub.Building.Attributes;
using Microsoft.Extensions.Logging;

namespace MultiLayerProceduralResources;

[AutoTimestampVersion("yyyyMMddHHmmss")]
public sealed partial class EmittingResourceProcessor : Processor<EmittingResourceDTO, EmittingResourceDTO> {
    protected override EmittingResourceDTO Process(EmittingResourceDTO importedObject, ProcessingContext context) {
        if (importedObject.Count > 0) {
            context.ProceduralResources.Add(new() {
                Object = new EmittingResourceDTO {
                    Value = importedObject.Value,
                    Count = importedObject.Count - 1,
                },
                ProcessorName = nameof(EmittingResourceProcessor),
            }, out _);
        }

        return importedObject;
    }
}