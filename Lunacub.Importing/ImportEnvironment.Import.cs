namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    private readonly ResourceCache _resourceCache;

    /// <summary>
    /// Imports the weakly-typed resource associates with <paramref name="rid"/> along with its dependencies.
    /// </summary>
    /// <param name="rid">Id of the resource to import.</param>
    /// <returns>The <see cref="ImportingOperation"/> handle of the operation.</returns>
    public ImportingOperation Import(ResourceID rid) {
        return _resourceCache.ImportAsync(rid);
    }

    /// <summary>
    /// Imports the strongly-typed resource associates with <paramref name="rid"/> along with its dependencies.
    /// </summary>
    /// <param name="rid">Id of the resource to import.</param>
    /// <typeparam name="T">Resource type to import. Must be a reference type.</typeparam>
    /// <returns>The <see cref="ImportingOperation{T}"/> handle of the operation.</returns>
    public ImportingOperation<T> Import<T>(ResourceID rid) where T : class {
        return _resourceCache.ImportAsync<T>(rid);
    }

    /// <summary>
    /// Releases the resource using the specified imported resource object.
    /// </summary>
    /// <param name="resource">The resource object to release.</param>
    /// <returns>The status of releasing operation.</returns>
    public ReleaseStatus Release(object? resource) {
        if (resource is null) return ReleaseStatus.Null;
        
        return _resourceCache.Release(resource);
    }
    
    /// <summary>
    /// Releases the resource using the specified resource handle.
    /// </summary>
    /// <param name="handle">The resource handle to release.</param>
    /// <returns>The status of releasing operation.</returns>
    /// <seealso cref="ResourceHandle"/>
    /// <seealso cref="ResourceHandle{T}"/>
    public ReleaseStatus Release(ResourceHandle handle) {
        if (handle.Rid == ResourceID.Null || handle.Value == null) return ReleaseStatus.Null;

        return _resourceCache.Release(handle);
    }

    /// <summary>
    /// Releases the resource using the specified resource Id.
    /// </summary>
    /// <param name="rid">The Id of the resource to release.</param>
    /// <returns>The status of releasing operation.</returns>
    /// <seealso cref="ResourceID"/>
    public ReleaseStatus Release(ResourceID rid) {
        if (rid == ResourceID.Null) return ReleaseStatus.Null;
        
        return _resourceCache.Release(rid);
    }
}