namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    private readonly ResourceImportDispatcher _importDispatcher;

    /// <summary>
    /// Imports the weakly-typed resource associates with <paramref name="rid"/> along with its references.
    /// </summary>
    /// <param name="rid">Id of the resource to import.</param>
    /// <returns>The <see cref="ImportingOperation"/> instance that encapsulates the importing operation.</returns>
    public ImportingOperation Import(ResourceID rid) {
        return _importDispatcher.Import(rid);
    }

    /// <summary>
    /// Imports the weakly-typed resource associates with <see cref="name"/> along with its references.
    /// </summary>
    /// <param name="name">Name of the resource to import.</param>
    /// <returns>The <see cref="ImportingOperation"/> instance that encapsulates the importing operation.</returns>
    public ImportingOperation Import(ReadOnlySpan<char> name) {
        return _importDispatcher.Import(name);
    }

    // TODO: Generic overload.
    // /// <summary>
    // /// Imports the strongly-typed resource associates with <paramref name="rid"/> along with its dependencies.
    // /// </summary>
    // /// <param name="rid">Id of the resource to import.</param>
    // /// <typeparam name="T">Resource type to import. Must be a reference type.</typeparam>
    // /// <returns>The <see cref="ImportingOperation{T}"/> handle of the operation.</returns>
    // public ImportingOperation<T> Import<T>(ResourceID rid) where T : class {
    //     return _importDispatcher.Import<T>(rid);
    // }

    /// <summary>
    /// Releases the resource using the specified imported resource object.
    /// </summary>
    /// <param name="resource">The resource object to release.</param>
    /// <returns>The status of releasing operation.</returns>
    public ReleaseStatus Release(object? resource) {
        return _importDispatcher.Release(resource);
    }
    
    /// <summary>
    /// Releases the resource using the specified resource handle.
    /// </summary>
    /// <param name="handle">The resource handle to release.</param>
    /// <returns>The status of releasing operation.</returns>
    /// <seealso cref="ResourceHandle"/>
    /// <seealso cref="ResourceHandle{T}"/>
    public ReleaseStatus Release(ResourceHandle handle) {
        return _importDispatcher.Release(handle);
    }

    /// <summary>
    /// Releases the resource using the specified resource Id.
    /// </summary>
    /// <param name="resourceId">The Id of the resource to release.</param>
    /// <returns>The status of releasing operation.</returns>
    /// <seealso cref="ResourceID"/>
    public ReleaseStatus Release(ResourceID resourceId) {
        return _importDispatcher.Release(resourceId);
    }

    /// <summary>
    /// Releases the importing resource using the provided <see cref="ImportingOperation"/> instance.
    /// </summary>
    /// <param name="operation">The instance of <see cref="ImportingOperation"/> to release.</param>
    /// <returns>The status of releasing operation.</returns>
    public ReleaseStatus Release(ImportingOperation operation) {
        return _importDispatcher.Release(operation);
    }
}