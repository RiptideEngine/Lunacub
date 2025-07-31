namespace Caxivitual.Lunacub;

/// <summary>
/// Represents a lightweight, immutable handle that identify the underlying imported object.
/// </summary>
/// <seealso cref="ResourceHandle{T}"/>
public readonly struct ResourceHandle : IEquatable<ResourceHandle> {
    public readonly ResourceAddress Address;
    public readonly object? Value;
    
    /// <summary>
    /// Creates new instance of <see cref="ResourceHandle"/> with the specified <see cref="ResourceAddress"/> and an underlying object.
    /// </summary>
    /// <param name="address">Address of the resource.</param>
    /// <param name="value">Underlying resource object of the handle.</param>
    public ResourceHandle(ResourceAddress address, object? value) {
        Address = address;
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
    ///     A new instance of <see cref="ResourceHandle{T}"/> with the same <see cref="Address"/> and <see cref="Value"/>
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
        return new(Address, (T)Value!);
    }
    
    /// <summary>
    /// Converts the underlying resource object to a strongly-typed instance of <see cref="ResourceHandle{T}"/> specified
    /// by type parameter.
    /// </summary>
    /// <typeparam name="T">
    ///     The type to which the underlying resource object is converted. Must be a reference type.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="ResourceHandle{T}"/> with the same <see cref="Address"/> and <see cref="Value"/>
    ///     with the specified type.
    /// </returns>
    /// <remarks>
    ///     The returned <see cref="ResourceHandle{T}"/> will have the same <see cref="Value"/> instance as the original
    ///     one if casting succeeds. Otherwise, <see cref="Value"/> will be <see langword="null"/>.
    /// </remarks>
    public ResourceHandle<T> Convert<T>() where T : class {
        return new(Address, Value as T);
    }

    /// <summary>
    /// Deconstruct the <see cref="ResourceHandle"/> into separated variables.
    /// </summary>
    /// <param name="address">When this method returns, contains the <see cref="Address"/> of this handle.</param>
    /// <param name="value">When this method returns, contains the resource object of this handle.</param>
    public void Deconstruct(out ResourceAddress address, out object? value) {
        address = Address;
        value = Value;
    }
    
    /// <summary>
    /// Determines whether this instance and other <see cref="ResourceHandle"/> are equal.
    /// </summary>
    /// <param name="other">The other <see cref="ResourceHandle"/> to compare to.</param>
    /// <returns>
    ///     <see langword="true"/> if the <see cref="Address"/> and <see cref="Value"/> of both handles are equal.
    /// </returns>
    public bool Equals(ResourceHandle other) => other.Address == Address && Value == other.Value;

    /// <summary>
    /// Determines whether this instance and a specified object, which must also be a <see cref="ResourceHandle"/>,
    /// are equal.
    /// </summary>
    /// <param name="obj">The object to compare to.</param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="obj"/> is a <see cref="ResourceHandle"/> and its <see cref="Address"/>
    ///     and <see cref="Value"/> are equal to the instance; otherwise, <see langword="false"/>.
    /// </returns>
    /// <seealso cref="Equals(ResourceHandle)"/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ResourceHandle other && Equals(other);
    
    [ExcludeFromCodeCoverage] public override int GetHashCode() => HashCode.Combine(Address, Value);
    
    public static bool operator ==(ResourceHandle left, ResourceHandle right) => left.Equals(right);
    public static bool operator !=(ResourceHandle left, ResourceHandle right) => !left.Equals(right);
}