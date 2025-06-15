using System.Text;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public sealed partial class EmittableResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ProcessedEmittableResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer {
        public override string DeserializerName => nameof(EmittableResourceDeserializer);
        
        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) { }

        public override void SerializeObject(Stream outputStream) {
            ProcessedEmittableResourceDTO dto = (ProcessedEmittableResourceDTO)SerializingObject;

            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(dto.Value);
            writer.Write(dto.GeneratedId);
        }
    }
}