namespace MultiLayerProceduralResources;

public record EmittingResource(int Value);

public sealed class EmittingResourceDTO : ContentRepresentation {
    public int Value;
    public int Count;

    public EmittingResourceDTO(int value, int count) {
        Value = value;
        Count = count;
    }
}