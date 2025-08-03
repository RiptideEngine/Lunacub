using Microsoft.Extensions.Logging;
using System.Text;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public sealed class EmittableResourceDeserializer : Deserializer<EmittableResource> {
    protected override Task<EmittableResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        int value = reader.ReadInt32();
        var generatedResourceAddress = reader.ReadResourceAddress();
        
        context.Logger.LogDebug("Requesting generated resource address: {addrL} - {addrR}", generatedResourceAddress.LibraryId, generatedResourceAddress.ResourceId.ToString("X"));
        
        context.RequestReference(1, new(generatedResourceAddress));
        
        return Task.FromResult(new EmittableResource {
            Value = value,
            Generated = null,
        });
    }

    protected override void ResolveReferences(EmittableResource instance, DeserializationContext context) {
        var referenceHandle = context.GetReference(1);

        if (referenceHandle.Value is SimpleResource reference) {
            instance.Generated = reference;
        } else {
            context.ReleaseReference(1);
        }
    }
}