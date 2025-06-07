using System.Text;

namespace Caxivitual.Lunacub.Examples.ReferenceImporting;

public sealed class ReferenceResourceDeserializer : Deserializer<ReferenceResource> {
    public override bool Streaming => false;

    protected override Task<ReferenceResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using BinaryReader br = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        context.RequestReference<ReferenceResource>(1, br.ReadResourceID());
        return Task.FromResult(new ReferenceResource { Value = br.ReadInt32() });
    }

    protected override void ResolveReferences(ReferenceResource instance, DeserializationContext context) {
        instance.Reference = context.GetReference<ReferenceResource>(1);
    }
}