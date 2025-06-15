using Microsoft.Extensions.Logging;
using System.Text;

namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

public class ReferencingResourceDeserializer : Deserializer<ReferencingResource> {
    protected override Task<ReferencingResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        ResourceID referenceId = reader.ReadResourceID();
        
        context.RequestReference<SimpleResource>(0, referenceId);
        
        return Task.FromResult(new ReferencingResource());
    }

    protected override void ResolveReferences(ReferencingResource instance, DeserializationContext context) {
        instance.Reference = context.GetReference<SimpleResource>(0);
    }
}