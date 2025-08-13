namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that generates an instance of <see cref="Serializer"/> requested by the <see cref="BuildEnvironment"/>.
/// </summary>
public abstract class SerializerFactory {
    /// <summary>
    /// Determines whether a type of <see cref="object"/> can be serialized by this factory.
    /// </summary>
    /// <param name="resourceType">Type of <see cref="object"/> to check for.</param>
    /// <returns>
    ///     <see langword="true"/> if this factory can returns a suitable <see cref="Serializer"/> to serialize the
    ///     <see cref="object"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool CanSerialize(Type resourceType);

    internal Serializer InternalCreateSerializer(object serializingObject, SerializationContext context) {
        if (CreateUntypedSerializer(serializingObject, context) is not { } serializer) {
            throw new InvalidOperationException($"{nameof(SerializerFactory)} does not allows null {nameof(Serializer)} to be returned.");
        }
        
        return serializer;
    }

    /// <summary>
    /// Generates a suitable <see cref="Serializer"/> to serialize the specified <see cref="object"/>.
    /// </summary>
    /// <param name="serializingObject">The object that is being serialized.</param>
    /// <param name="context">The context associates with the serialization process.</param>
    /// <returns>
    ///     An instance of <see cref="Serializer"/> that suitable for serializing <paramref name="serializingObject"/>.
    /// </returns>
    protected abstract Serializer CreateUntypedSerializer(object serializingObject, SerializationContext context);
}

/// <summary>
/// Provides the base class that generates an instance of <see cref="Serializer{T}"/> requested by the <see cref="BuildEnvironment"/>.
/// </summary>
/// <typeparam name="T">The type extends <see cref="object"/>.</typeparam>
public abstract class SerializerFactory<T> : SerializerFactory {
    public sealed override bool CanSerialize(Type resourceType) {
        return resourceType.IsAssignableTo(typeof(T));
    }

    protected sealed override Serializer CreateUntypedSerializer(object serializingObject, SerializationContext context) {
        return CreateSerializer(serializingObject, context);
    }

    /// <summary>
    /// Generates a suitable <see cref="Serializer{T}"/> to serialize the specified instance of <see cref="object"/>.
    /// </summary>
    /// <param name="serializingObject">The object that is being serialized.</param>
    /// <param name="context">The context associates with the serialization process.</param>
    /// <returns>
    ///     An instance of <see cref="Serializer{T}"/> that suitable for serializing <paramref name="serializingObject"/>.
    /// </returns>
    protected abstract Serializer<T> CreateSerializer(object serializingObject, SerializationContext context);
}