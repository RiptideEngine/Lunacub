namespace Caxivitual.Lunacub;

/// <summary>
/// Represents a lightweight, immutable handle that identify the underlying imported object.
/// </summary>
/// <seealso cref="ResourceHandle{T}"/>
/// <seealso cref="ResourceID"/>
public readonly struct ResourceHandle : IEquatable<ResourceHandle> {
    public readonly ResourceID ResourceId;
    public readonly object? Value;
    
    /// <summary>
    /// Creates new instance of <see cref="ResourceHandle"/> with the specified <see cref="ResourceID"/> and underlying object.
    /// </summary>
    /// <param name="resourceId">Id of the handle.</param>
    /// <param name="value">Underlying resource object of the handle.</param>
    public ResourceHandle(ResourceID resourceId, object? value) {
        ResourceId = resourceId;
        Value = value;
    }

    /// <summary>
    /// Converts the underlying resource object to a strongly-typed handle <see cref="ResourceHandle{T}"/> specified
    /// by type parameter in an unsafe manner.
    /// </summary>
    /// <typeparam name="T">
    ///     The type to which the underlying resource object is converted. Must be a reference type.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="ResourceHandle{T}"/> with the same <see cref="ResourceId"/> and <see cref="Value"/>
    ///     with the specified type.
    /// </returns>
    /// <exception cref="InvalidCastException">
    ///     The underlying object cannot be casted to <typeparamref name="T"/>.
    /// </exception>
    /// <remarks>
    ///     The returned <see cref="ResourceHandle{T}"/> will have the same <see cref="Value"/> instance as the original
    ///     one if casting succeeds.
    /// </remarks>
    public ResourceHandle<T> ConvertUnsafe<T>() where T : class {
        return new(ResourceId, (T)Value!);
    }
    
    /// <summary>
    /// Converts the underlying resource object to a strongly-typed instance of <see cref="ResourceHandle{T}"/> specified
    /// by type parameter.
    /// </summary>
    /// <typeparam name="T">
    ///     The type to which the underlying resource object is converted. Must be a reference type.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="ResourceHandle{T}"/> with the same <see cref="ResourceId"/> and <see cref="Value"/>
    ///     with the specified type.
    /// </returns>
    /// <remarks>
    ///     The returned <see cref="ResourceHandle{T}"/> will have the same <see cref="Value"/> instance as the original
    ///     one if casting succeeds. Otherwise, <see cref="Value"/> will be <see langword="null"/>.
    /// </remarks>
    public ResourceHandle<T> Convert<T>() where T : class {
        return new(ResourceId, Value as T);
    }

    public void Deconstruct(out ResourceID resourceId, out object? value) {
        resourceId = ResourceId;
        value = Value;
    }
    
    /// <summary>
    /// Determines whether this instance and other <see cref="ResourceHandle"/> are equal.
    /// </summary>
    /// <param name="other">The other <see cref="ResourceHandle"/> to compare to.</param>
    /// <returns>
    ///     <see langword="true"/> if the <see cref="ResourceId"/> and <see cref="Value"/> of both handles are equal.
    /// </returns>
    public bool Equals(ResourceHandle other) => other.ResourceId == ResourceId && Value == other.Value;

    /// <summary>
    /// Determines whether this instance and a specified object, which must also be a <see cref="ResourceHandle"/>,
    /// are equal.
    /// </summary>
    /// <param name="obj">The object to compare to.</param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="obj"/> is a <see cref="ResourceHandle"/> and its <see cref="ResourceId"/>
    ///     and <see cref="Value"/> are equal to the instance; otherwise, <see langword="false"/>.
    /// </returns>
    /// <seealso cref="Equals(ResourceHandle)"/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ResourceHandle other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(ResourceId, Value);
    
    public static bool operator ==(ResourceHandle left, ResourceHandle right) => left.Equals(right);
    public static bool operator !=(ResourceHandle left, ResourceHandle right) => !left.Equals(right);
}