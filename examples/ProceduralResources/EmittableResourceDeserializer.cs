using System.Text;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public sealed class EmittableResourceDeserializer : Deserializer<EmittableResource> {
    protected override Task<EmittableResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        int value = reader.ReadInt32();
        ResourceID generatedId = reader.ReadResourceID();
        
        context.RequestReference<SimpleResource>(0, generatedId);
        
        return Task.FromResult(new EmittableResource() {
            Value = value,
            Generated = null,
        });
    }

    protected override void ResolveReferences(EmittableResource instance, DeserializationContext context) {
        instance.Generated = context.GetReference<SimpleResource>(0);
    }
}