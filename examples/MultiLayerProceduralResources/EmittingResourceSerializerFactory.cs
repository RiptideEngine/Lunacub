using System.Text;

namespace MultiLayerProceduralResources;

public sealed partial class EmittingResourceSerializerFactory : SerializerFactory<EmittingResourceDTO> {
    protected override Serializer<EmittingResourceDTO> CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer<EmittingResourceDTO> {
        public override string DeserializerName => nameof(EmittingResourceDeserializer);
        
        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) { }

        public override void SerializeObject(Stream outputStream) {
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(SerializingObject.Value);
        }
    }
}