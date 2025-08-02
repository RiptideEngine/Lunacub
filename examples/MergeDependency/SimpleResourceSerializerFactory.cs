using System.Text;

namespace Caxivitual.Lunacub.Examples.MergeDependency;

public sealed partial class SimpleResourceSerializerFactory : SerializerFactory<SimpleResourceDTO> {
    protected override Serializer<SimpleResourceDTO> CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer<SimpleResourceDTO> {
        public override string DeserializerName => nameof(SimpleResourceDeserializer);
        
        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) { }

        public override void SerializeObject(Stream outputStream) {
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(SerializingObject.Value);
        }
    }
}