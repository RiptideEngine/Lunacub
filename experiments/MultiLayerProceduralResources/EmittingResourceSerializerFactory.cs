using System.Text;

namespace MultiLayerProceduralResources;

public sealed partial class EmittingResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(EmittingResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer {
        public override string DeserializerName => nameof(EmittingResourceDeserializer);
        
        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) { }

        public override void SerializeObject(Stream outputStream) {
            EmittingResourceDTO dto = (EmittingResourceDTO)SerializingObject;

            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(dto.Value);
        }
    }
}