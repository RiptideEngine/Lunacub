using System.Text;

namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

public sealed class TextureSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(Texture);
    
    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(TextureDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
            var texture = (TextureDTO)SerializingObject;
            
            writer.Write(texture.Name);
            writer.Write(texture.Sprites.Length);

            foreach (var sprite in texture.Sprites) {
                writer.Write(sprite.Name);
                writer.WriteReinterpret(sprite.Rect);
            }
        }
    }
}