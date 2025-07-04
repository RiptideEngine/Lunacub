﻿using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Building.Attributes;
using SixLabors.ImageSharp;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

public sealed class Texture2DSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(Texture2DDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => "Texture2DDeserializer";
        
        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            Texture2DDTO dto = (Texture2DDTO)SerializingObject;
            
            dto.Image.SaveAsQoi(outputStream);
        }
    }
}