namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ExceptionThrowingSerializerFactory<T> : SerializerFactory<T> {
    protected override Serializer<T> CreateSerializer(T serializingObject, SerializationContext context) {
        throw new("This exception is expected.");
    }
}