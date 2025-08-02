using System.Text;

namespace Caxivitual.Lunacub.Examples.MergeDependency;

public sealed partial class MergingResourceSerializerFactory : SerializerFactory<ProcessedMergingResourceDTO> {
    protected override Serializer<ProcessedMergingResourceDTO> CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private partial class SerializerCore : Serializer<ProcessedMergingResourceDTO> {
        public override string DeserializerName => nameof(MergingResourceDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) { }

        public override void SerializeObject(Stream outputStream) {
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            ProcessedMergingResourceDTO dto = SerializingObject;
            
            writer.Write7BitEncodedInt(dto.Values.Length);

            foreach (var value in dto.Values) {
                writer.Write(value);
            }
        }
    }
}