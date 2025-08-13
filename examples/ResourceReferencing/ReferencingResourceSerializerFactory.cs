using Microsoft.Extensions.Logging;
using System.Text;

namespace Caxivitual.Lunacub.Examples.ResourceReferencing;

public class ReferencingResourceSerializerFactory : SerializerFactory<ReferencingResourceDTO> {
    protected override Serializer<ReferencingResourceDTO> CreateSerializer(ReferencingResourceDTO serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed partial class SerializerCore : Serializer<ReferencingResourceDTO> {
        public override string DeserializerName => nameof(ReferencingResourceDeserializer);
        
        public SerializerCore(ReferencingResourceDTO obj, SerializationContext context) : base(obj, context) { }

        public override void SerializeObject(Stream outputStream) {
            using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
            writer.Write(SerializingObject.Reference);
        }
    }
}