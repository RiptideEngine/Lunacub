namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public record EmittableResource {
    public int Value;
    public SimpleResource? Generated;
}

public sealed class EmittableResourceDTO {
    public int Value { get; set; }
}

public sealed class ProcessedEmittableResourceDTO {
    public int Value;
    public ResourceAddress GeneratedAddress;

    public ProcessedEmittableResourceDTO(int value, ResourceAddress generatedId) {
        Value = value;
        GeneratedAddress = generatedId;
    }
}