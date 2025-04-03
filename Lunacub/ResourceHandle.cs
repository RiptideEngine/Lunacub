using System.Runtime.CompilerServices;

namespace Caxivitual.Lunacub;

public readonly record struct ResourceHandle(ResourceID Rid, object? Value) {
    public ResourceHandle<T> Convert<T>() where T : class {
        return new(Rid, (T)Value!);
    }
    
    public ResourceHandle<T> ConvertUnchecked<T>() where T : class {
        return new(Rid, Value as T);
    }
}

public readonly record struct ResourceHandle<T>(ResourceID Rid, T? Value) where T : class {
    public ResourceHandle<TOther> Convert<TOther>() where TOther : class {
        return new(Rid, (TOther)(object)Value!);
    }
    
    public ResourceHandle<TOther> ConvertUnchecked<TOther>() where TOther : class {
        return new(Rid, Value as TOther);
    }
    
    public static implicit operator ResourceHandle(ResourceHandle<T> handle) => Unsafe.As<ResourceHandle<T>, ResourceHandle>(ref handle);
}