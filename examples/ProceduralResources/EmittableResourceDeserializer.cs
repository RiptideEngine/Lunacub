using System.Text;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public sealed class EmittableResourceDeserializer : Deserializer<EmittableResource> {
    protected override Task<EmittableResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        int value = reader.ReadInt32();
        
        return Task.FromResult(new EmittableResource(value));
    }
}