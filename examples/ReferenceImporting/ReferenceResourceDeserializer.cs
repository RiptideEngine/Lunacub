﻿using System.Text;

namespace ReferenceImporting;

public sealed class ReferenceResourceDeserializer : Deserializer<ReferenceResource> {
    public override bool Streaming => false;

    protected override ReferenceResource Deserialize(Stream dataStream, Stream optionStream, DeserializationContext context) {
        using BinaryReader br = new BinaryReader(dataStream, Encoding.UTF8, true);
        
        context.RequestReference<ReferenceResource>(nameof(ReferenceResource.Reference), br.ReadResourceID());
        return new() {
            Value = br.ReadInt32(),
        };
    }

    protected override void ResolveDependencies(ReferenceResource instance, DeserializationContext context) {
        instance.Reference = context.GetReference<ReferenceResource>(nameof(ReferenceResource.Reference));
    }
}