namespace Caxivitual.Lunacub.Building;

public abstract class Serializer {
    public ContentRepresentation SerializingObject { get; }
    public SerializationContext Context { get; }
    
    public abstract string DeserializerName { get; }

    protected Serializer(ContentRepresentation serializingObject, SerializationContext context) {
        SerializingObject = serializingObject;
        Context = context;
    }
    
    public abstract void SerializeObject(Stream outputStream);
    public virtual void SerializeOptions(Stream outputStream) {
    }
}