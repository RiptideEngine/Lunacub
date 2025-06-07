using System.Text;

namespace Caxivitual.Lunacub.Examples.SimpleImporting;

public sealed partial class SimpleResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(SimpleResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer {
        public override string DeserializerName => nameof(SimpleResourceDeserializer);
        
        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) { }

        public override void SerializeObject(Stream outputStream) {
            SimpleResourceDTO dto = (SimpleResourceDTO)SerializingObject;

            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(dto.Integer);
            writer.Write(dto.Single);
            writer.Write(dto.Vector);
        }
    }
}