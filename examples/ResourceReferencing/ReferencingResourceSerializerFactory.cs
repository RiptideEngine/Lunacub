using Microsoft.Extensions.Logging;
using System.Text;

namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

public class ReferencingResourceSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(ReferencingResourceDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer {
        public override string DeserializerName => nameof(ReferencingResourceDeserializer);
        
        public SerializerCore(ContentRepresentation contentRepresentation, SerializationContext context) : base(contentRepresentation, context) { }

        public override void SerializeObject(Stream outputStream) {
            ReferencingResourceDTO dto = (ReferencingResourceDTO)SerializingObject;

            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(dto.ReferenceId);
        }
    }
}