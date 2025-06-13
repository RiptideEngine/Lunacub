namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public record EmittableResource(int Value);

public sealed class EmittableResourceDTO : ContentRepresentation {
    public int Value;

    public EmittableResourceDTO(int value) {
        Value = value;
    }
}