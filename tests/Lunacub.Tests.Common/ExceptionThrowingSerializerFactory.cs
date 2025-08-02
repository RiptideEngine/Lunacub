namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ExceptionThrowingSerializerFactory<T> : SerializerFactory<T> where T : ContentRepresentation {
    protected override Serializer<T> CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        throw new("This exception is expected.");
    }
}