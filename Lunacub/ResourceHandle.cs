using System.Runtime.CompilerServices;

namespace Caxivitual.Lunacub;

public readonly struct ResourceHandle : IEquatable<ResourceHandle> {
    public readonly ResourceID Rid;
    public readonly object? Value;
    
    public ResourceHandle(ResourceID rid, object? value) {
        Rid = rid;
        Value = value;
    }
    
    public ResourceHandle<T> ConvertUnsafe<T>() where T : class {
        return new(Rid, (T)Value!);
    }
    
    public ResourceHandle<T> Convert<T>() where T : class {
        return new(Rid, Value as T);
    }

    public bool Equals(ResourceHandle other) => other.Rid == Rid && Value == other.Value;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ResourceHandle other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(Rid, Value);
    
    public static bool operator ==(ResourceHandle left, ResourceHandle right) => left.Equals(right);
    public static bool operator !=(ResourceHandle left, ResourceHandle right) => !left.Equals(right);
}

public readonly struct ResourceHandle<T> : IEquatable<ResourceHandle<T>> where T : class {
    public readonly ResourceID Rid;
    public readonly T? Value;

    public ResourceHandle(ResourceID rid, T? value) {
        Rid = rid;
        Value = value;
    }
    
    public ResourceHandle<TOther> ConvertUnsafe<TOther>() where TOther : class {
        return new(Rid, (TOther?)(object?)Value);
    }
    
    public ResourceHandle<TOther> Convert<TOther>() where TOther : class {
        return new(Rid, Value as TOther);
    }
    
    public bool Equals(ResourceHandle<T> other) => other.Rid == Rid && Value == other.Value;
    
    public static implicit operator ResourceHandle(ResourceHandle<T> handle) => Unsafe.As<ResourceHandle<T>, ResourceHandle>(ref handle);
    
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ResourceHandle<T> other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(Rid, Value);
    
    public static bool operator ==(ResourceHandle<T> left, ResourceHandle<T> right) => left.Equals(right);
    public static bool operator !=(ResourceHandle<T> left, ResourceHandle<T> right) => !left.Equals(right);
}