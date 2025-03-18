using Caxivitual.Lunacub.Compilation;
using Caxivitual.Lunacub.Exceptions;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Caxivitual.Lunacub.Importing;

public sealed partial class ResourceRegistry : IDisposable {
    private readonly ReaderWriterLockSlim _lock;
    private readonly Dictionary<ResourceID, object> _resourceCache;
    private readonly ImportEnvironment _context;
    
    private bool _disposed;

    internal ResourceRegistry(ImportEnvironment context) {
        _lock =  new(LockRecursionPolicy.SupportsRecursion);
        _resourceCache = [];
        _context = context;
    }
    
    internal T? Import<T>(ResourceID rid) where T : class {
        return Import(rid, typeof(T)) as T;
    }

    private object? Import(ResourceID rid, Type type) {
        _lock.EnterUpgradeableReadLock();
        try {
            if (_resourceCache.TryGetValue(rid, out var cache)) {
                return cache.GetType().IsAssignableTo(type) ? cache : null;
            }
            
            _lock.EnterWriteLock();
            try {
                return ImportInner(rid, type);
            } finally {
                _lock.ExitWriteLock();
            }
        } finally {
            _lock.ExitUpgradeableReadLock();
        }
    }
    
    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;

        if (disposing) {
            // TODO: Dispose Resources.
            
            _lock.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ResourceRegistry() {
        Dispose(false);
    }
}