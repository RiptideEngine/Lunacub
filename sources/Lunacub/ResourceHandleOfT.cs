namespace Caxivitual.Lunacub;

/// <summary>
/// Represents a lightweight, immutable handle that identify the underlying imported object.
/// </summary>
/// <seealso cref="ResourceHandle"/>
public readonly struct ResourceHandle<T> : IEquatable<ResourceHandle<T>> where T : class {
    public readonly ResourceAddress Address;
    public readonly T? Value;

    /// <summary>
    /// Creates new instance of <see cref="ResourceHandle{T}"/> with the specified <see cref="ResourceID"/> and underlying object.
    /// </summary>
    /// <param name="address">Address of the resource.</param>
    /// <param name="value">Underlying resource object of the handle.</param>
    public ResourceHandle(ResourceAddress address, T? value) {
        Address = address;
        Value = value;
    }
    
    /// <summary>
    /// Converts the underlying resource object to another strongly-typed handle <see cref="ResourceHandle{T}"/> specified
    /// by type parameter in an unsafe manner.
    /// </summary>
    /// <typeparam name="TOther">
    ///     The type to which the underlying resource object is converted. Must be a reference type.
    /// </typeparam>
    /// <returns>
    ///     A new instance of <see cref="ResourceHandle{T}"/> with the same <see cref="Address"/> and <see cref="Value"/>
    ///     with the specified type.
    /// </returns>
    /// <exception cref="InvalidCastException">
    ///     The underlying object cannot be casted to <typeparamref name="TOther"/>.
    /// </exception>
    /// <remarks>
    ///     The returned <see cref="ResourceHandle{T}"/> will have the same <see cref="Value"/> instance as the original
    ///     one if casting succeeds.
    /// </remarks>
    public ResourceHandle<TOther> ConvertUnsafe<TOther>() where TOther : class {
        return new(Address, (TOther?)(object?)Value);
    }
    
    /// <summary>
    /// Converts the underlying resource object to another strongly-typed handle <see cref="ResourceHandle{T}"/> specified
    /// by type parameter.
    /// </summary>
    /// <typeparam name="TOther">
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
    public ResourceHandle<TOther> Convert<TOther>() where TOther : class {
        return new(Address, Value as TOther);
    }
    
    /// <inheritdoc cref="ResourceHandle.Deconstruct"/>
    public void Deconstruct(out ResourceAddress address, out T? value) {
        address = Address;
        value = Value;
    }
    
    /// <summary>
    /// Determines whether this instance and other <see cref="ResourceHandle{T}"/> are equal.
    /// </summary>
    /// <param name="other">The other <see cref="ResourceHandle{T}"/> to compare to.</param>
    /// <returns>
    ///     <see langword="true"/> if the <see cref="Address"/> and <see cref="Value"/> of both handles are equal.
    /// </returns>
    public bool Equals(ResourceHandle<T> other) => other.Address == Address && Value == other.Value;
    
    /// <summary>
    /// Converts this instance into an instance of weakly-typed <see cref="ResourceHandle"/>.
    /// </summary>
    /// <param name="handle">The original <see cref="ResourceHandle{T}"/> to convert.</param>
    /// <returns></returns>
    public static implicit operator ResourceHandle(ResourceHandle<T> handle) => Unsafe.As<ResourceHandle<T>, ResourceHandle>(ref handle);
    
    /// <summary>
    /// Determines whether this instance and a specified object, which must also be a <see cref="ResourceHandle{T}"/>,
    /// are equal.
    /// </summary>
    /// <param name="obj">The object to compare to.</param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="obj"/> is a <see cref="ResourceHandle"/> and its <see cref="Address"/>
    ///     and <see cref="Value"/> are equal to the instance; otherwise, <see langword="false"/>.
    /// </returns>
    /// <seealso cref="Equals(ResourceHandle{T})"/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ResourceHandle<T> other && Equals(other);
    
    [ExcludeFromCodeCoverage] public override int GetHashCode() => HashCode.Combine(Address, Value);
    
    public static bool operator ==(ResourceHandle<T> left, ResourceHandle<T> right) => left.Equals(right);
    public static bool operator !=(ResourceHandle<T> left, ResourceHandle<T> right) => !left.Equals(right);
}