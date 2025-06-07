using System.Text;

namespace Caxivitual.Lunacub.Examples.ReferenceImporting;

public sealed partial class ReferenceResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ReferenceResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer {
        public override string DeserializerName => nameof(ReferenceResourceDeserializer);
        
        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) { }

        public override void SerializeObject(Stream outputStream) {
            ReferenceResourceDTO dto = (ReferenceResourceDTO)SerializingObject;
            
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(dto.Reference);
            writer.Write(dto.Value);
        }
    }
}