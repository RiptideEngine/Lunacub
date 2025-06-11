using System.Text;

namespace Caxivitual.Lunacub.Examples.DependencyImporting;

public sealed partial class MergingResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ProcessedMergingResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private partial class SerializerCore : Serializer {
        public override string DeserializerName => nameof(MergingResourceDeserializer);

        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) { }

        public override void SerializeObject(Stream outputStream) {
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            ProcessedMergingResourceDTO dto = (ProcessedMergingResourceDTO)SerializingObject;
            
            writer.Write7BitEncodedInt(dto.Values.Length);

            foreach (var value in dto.Values) {
                writer.Write(value);
            }
        }
    }
}