using Caxivitual.Lunacub.Building.Attributes;

namespace MultiLayerProceduralResources;

public record EmittingResource(int Value);

[AutoTimestampVersion("yyyyMMddHHmmss")]
public sealed class EmittingResourceDTO : ContentRepresentation {
    public int Value;
    public int Count;

    public EmittingResourceDTO(int value, int count) {
        Value = value;
        Count = count;
    }

    public void Something() { }
}