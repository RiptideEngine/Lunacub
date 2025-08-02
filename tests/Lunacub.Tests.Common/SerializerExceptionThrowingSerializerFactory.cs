namespace Caxivitual.Lunacub.Tests.Common;

public sealed class SerializerExceptionThrowingSerializerFactory<T> : SerializerFactory<T> where T : ContentRepresentation {
    protected override Serializer<T> CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer<T> {
        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }
        public override string DeserializerName => string.Empty;

        public override void SerializeObject(Stream outputStream) {
            throw new("This exception is expected.");
        }
    }
}