using System.Numerics;
using System.Text;

namespace Caxivitual.Lunacub.Examples.DependencyImporting;

public sealed class SimpleResourceDeserializer : Deserializer<SimpleResource> {
    protected override Task<SimpleResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        int integer = reader.ReadInt32();
        
        return Task.FromResult(new SimpleResource(integer));
    }
}