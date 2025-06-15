using Microsoft.Extensions.Logging;

namespace MultiLayerProceduralResources;

public sealed class EmittingResourceProcessor : Processor<EmittingResourceDTO, EmittingResourceDTO> {
    protected override EmittingResourceDTO Process(EmittingResourceDTO importedObject, ProcessingContext context) {
        if (importedObject.Count > 0) {
            context.ProceduralResources.Add(1, new() {
                Object = new EmittingResourceDTO(importedObject.Value, importedObject.Count - 1),
                ProcessorName = nameof(EmittingResourceProcessor),
            });
        }

        return importedObject;
    }
}