namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public record EmittableResource {
    public int Value;
    public SimpleResource? Generated;
}

public sealed class EmittableResourceDTO : ContentRepresentation {
    public int Value { get; set; }
}

public sealed class ProcessedEmittableResourceDTO : ContentRepresentation {
    public int Value;
    public ResourceID GeneratedId;

    public ProcessedEmittableResourceDTO(int value, ResourceID generatedId) {
        Value = value;
        GeneratedId = generatedId;
    }
}