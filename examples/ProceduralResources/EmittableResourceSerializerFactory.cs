using System.Text;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public sealed partial class EmittableResourceSerializerFactory : SerializerFactory<ProcessedEmittableResourceDTO> {
    protected override Serializer<ProcessedEmittableResourceDTO> CreateSerializer(object serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer<ProcessedEmittableResourceDTO> {
        public override string DeserializerName => nameof(EmittableResourceDeserializer);
        
        public SerializerCore(object obj, SerializationContext context) : base(obj, context) { }

        public override void SerializeObject(Stream outputStream) {
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(SerializingObject.Value);
            writer.Write(SerializingObject.GeneratedAddress);
        }
    }
}