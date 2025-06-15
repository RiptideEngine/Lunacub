using System.Text;

namespace MultiLayerProceduralResources;

public sealed class EmittingResourceDeserializer : Deserializer<EmittingResource> {
    protected override Task<EmittingResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        int value = reader.ReadInt32();
        
        return Task.FromResult(new EmittingResource(value));
    }
}