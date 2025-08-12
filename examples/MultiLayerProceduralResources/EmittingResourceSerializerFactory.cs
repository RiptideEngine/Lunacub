using System.Text;

namespace MultiLayerProceduralResources;

public sealed partial class EmittingResourceSerializerFactory : SerializerFactory<EmittingResourceDTO> {
    protected override Serializer<EmittingResourceDTO> CreateSerializer(object serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer<EmittingResourceDTO> {
        public override string DeserializerName => nameof(EmittingResourceDeserializer);
        
        public SerializerCore(object obj, SerializationContext context) : base(obj, context) { }

        public override void SerializeObject(Stream outputStream) {
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(SerializingObject.Value);
        }
    }
}