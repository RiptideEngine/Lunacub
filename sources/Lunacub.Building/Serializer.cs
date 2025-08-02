namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the serialization process of a resource in form of a <see cref="ContentRepresentation"/>
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

/// <summary>
/// Provides the base class that handles the serialization process of a resource in form of type <typeparamref name="T"/> extends
/// <see cref="ContentRepresentation"/> from previous processing step.
/// </summary>
/// <typeparam name="T">The type extends <see cref="ContentRepresentation"/>.</typeparam>
public abstract class Serializer<T> : Serializer where T : ContentRepresentation {
    /// <summary>
    /// Gets the object that is being serialized.
    /// </summary>
    public new T SerializingObject => (T)base.SerializingObject;
    
    protected Serializer(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
    }
}