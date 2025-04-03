namespace Caxivitual.Lunacub.Building;

public abstract class SerializerFactory {
    public abstract bool CanSerialize(Type representationType);

    internal Serializer InternalCreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        if (CreateSerializer(serializingObject, context) is not { } serializer) {
            throw new InvalidOperationException("SerializerFactory does not allows null Serializer to be returned.");
        }
        
        return serializer;
    }

    protected abstract Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context);
}