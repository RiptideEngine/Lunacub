using System.Runtime.CompilerServices;

namespace Caxivitual.Lunacub;

public readonly record struct ResourceHandle(ResourceID Rid, object? Value) {
    public ResourceHandle<T> ConvertUnsafe<T>() {
        return new(Rid, (T)Value!);
    }
    
    public ResourceHandle<T> Convert<T>() {
        return new(Rid, Value is T t ? t : default);
    }
}

public readonly record struct ResourceHandle<T>(ResourceID Rid, T? Value) {
    public ResourceHandle<TOther> ConvertUnsafe<TOther>() {
        return new(Rid, (TOther)(object)Value!);
    }
    
    public ResourceHandle<TOther> Convert<TOther>() {
        return new(Rid, Value is TOther other ? other : default);
    }
    
    public static implicit operator ResourceHandle(ResourceHandle<T> handle) => Unsafe.As<ResourceHandle<T>, ResourceHandle>(ref handle);
}