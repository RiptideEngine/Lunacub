namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the serialization process of a resource as a <see cref="ContentRepresentation"/>
/// from previous processing step.
/// </summary>
/// <seealso cref="ContentRepresentation"/>
public abstract class Serializer {
    /// <summary>
    /// Gets the object that is being serialized.
    /// </summary>
    public ContentRepresentation SerializingObject { get; }
    
    /// <summary>
    /// Gets the context associates with the serialization process.
    /// </summary>
    public SerializationContext Context { get; }
    
    /// <summary>
    /// Gets the name of deserializer that will be used for importing process.
    /// </summary>
    public abstract string DeserializerName { get; }

    protected Serializer(ContentRepresentation serializingObject, SerializationContext context) {
        SerializingObject = serializingObject;
        Context = context;
    }
    
    /// <summary>
    /// Serializes the <see cref="ContentRepresentation"/> into a stream.
    /// </summary>
    /// <param name="outputStream">Output stream to receive the serialized data of <see cref="SerializingObject"/>.</param>
    public abstract void SerializeObject(Stream outputStream);
    
    /// <summary>
    /// Serializes the user provided options into a stream.
    /// </summary>
    /// <param name="outputStream">Output stream to receive the serialized options data.</param>
    public virtual void SerializeOptions(Stream outputStream) { }
}