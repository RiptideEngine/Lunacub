using System.Text;

namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

public sealed class TextureAtlasSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(TextureAtlasDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(TextureAtlasDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) { }

        public override void SerializeObject(Stream outputStream) {
            using BinaryWriter writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
            TextureAtlasDTO atlas = (TextureAtlasDTO)SerializingObject;
            
            writer.Write(atlas.Name);
            writer.WriteReinterpret<ResourceID>(atlas.Textures.AsSpan());
        }
    }
}