namespace Caxivitual.Lunacub.Importing;

partial class ImportEnvironment {
    private readonly ResourceImportDispatcher _importDispatcher;
    
    /// <summary>
    /// Imports the weakly-typed resource along with its references.
    /// </summary>
    /// <param name="address">Address of the resource to import.</param>
    /// <returns>The <see cref="ImportingOperation"/> instance that encapsulates the importing operation.</returns>
    public ImportingOperation Import(ResourceAddress address) {
        return _importDispatcher.Import(address);
    }
    
    /// <summary>
    /// Imports the weakly-typed resource along with its references.
    /// </summary>
    /// <param name="libraryId">Id of the library to import the resource with specified <paramref name="resourceId"/>.</param>
    /// <param name="resourceId">Id of the resource to import.</param>
    /// <returns>The <see cref="ImportingOperation"/> instance that encapsulates the importing operation.</returns>
    public ImportingOperation Import(LibraryID libraryId, ResourceID resourceId) {
        return Import(new ResourceAddress(libraryId, resourceId));
    }

    /// <summary>
    /// Imports the weakly-typed resource along with its references.
    /// </summary>
    /// <param name="address">Address of the resource to import.</param>
    /// <returns>The <see cref="ImportingOperation"/> instance that encapsulates the importing operation.</returns>
    public ImportingOperation Import(SpanNamedResourceAddress address) {
        return _importDispatcher.Import(address);
    }

    /// <summary>
    /// Imports the weakly-typed resource along with its references.
    /// </summary>
    /// <param name="libraryId">Id of the library to import the resource with specified <paramref name="name"/>.</param>
    /// <param name="name">Name of the resource to import.</param>
    /// <returns>The <see cref="ImportingOperation"/> instance that encapsulates the importing operation.</returns>
    public ImportingOperation Import(LibraryID libraryId, ReadOnlySpan<char> name) {
        return _importDispatcher.Import(new SpanNamedResourceAddress(libraryId, name));
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

    public IReadOnlyCollection<ImportingOperation> ImportByQueryTags(string query) {
        return ImportByQueryTags(new TagQuery(query));
    }

    public IReadOnlyCollection<ImportingOperation> ImportByQueryTags(TagQuery query) {
        return _importDispatcher.Import(query);
    }

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
    /// <param name="address">The address of resource to release.</param>
    /// <returns>The status of releasing operation.</returns>
    /// <seealso cref="ResourceID"/>
    public ReleaseStatus Release(ResourceAddress address) {
        return _importDispatcher.Release(address);
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