using Microsoft.Extensions.Logging;
using System.Text;

namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

public class ReferencingResourceDeserializer : Deserializer<ReferencingResource> {
    protected override Task<ReferencingResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        ResourceID referenceId = reader.ReadResourceID();
        
        context.RequestReference(1, new(referenceId));
        
        return Task.FromResult(new ReferencingResource());
    }

    protected override void ResolveReferences(ReferencingResource instance, DeserializationContext context) {
        ResourceHandle reference = context.GetReference(1);

        if (reference.Value is SimpleResource typedReference) {
            instance.Reference = typedReference;
        } else {
            context.ReleaseReference(reference);
        }
    }
}