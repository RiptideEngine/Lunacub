using Caxivitual.Lunacub.Building.Attributes;

namespace MultiLayerProceduralResources;

public record EmittingResource(int Value);

[AutoTimestampVersion("yyyyMMddHHmmss")]
public sealed class EmittingResourceDTO {
    public int Value { get; set; }
    public int Count { get; set; }
}