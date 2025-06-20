﻿namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public sealed class EmittableResourceProcessor : Processor<EmittableResourceDTO, EmittableResourceDTO> {
    protected override EmittableResourceDTO Process(EmittableResourceDTO importedObject, ProcessingContext context) {
        context.ProceduralResources.Add(1, new() {
            Object = new SimpleResourceDTO(importedObject.Value),
        });
        
        return importedObject;
    }
}