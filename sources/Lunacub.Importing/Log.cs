using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

[ExcludeFromCodeCoverage]
internal static partial class Log {
    // public static EventId BeginImportEvent => new(1, "BeginImport");
    // public static EventId ImportExceptionOccuredEvent => new(2, "ImportExceptionOccured");
    // public static EventId DependencyImportExceptionOccuredEvent => new(3, "DependencyImportExceptionOccured");
    // public static EventId ImportCancelEvent => new(4, "ImportCancel");
    // public static EventId ImportUnregisteredDependencyEvent => new(5, "ImportUnregisteredDependency");
    // public static EventId ResolveDependenciesEvent => new(6, "ResolveDependencies");

    [LoggerMessage(LogLevel.Information, "Begin import resource {rid}.")]
    public static partial void BeginImport(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Warning, "Trying to import an unregistered resource {rid}.")]
    public static partial void UnregisteredResource(ILogger logger, ResourceID rid);

    [LoggerMessage(LogLevel.Information, "Resource cached {rid} (Reference count: {referenceCount}).")]
    public static partial void CachedContainer(ILogger logger, ResourceID rid, uint referenceCount);

    [LoggerMessage(LogLevel.Information, "Begin resolving references for resource {rid}.")]
    public static partial void BeginResolvingReference(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Information, "End resolving references for resource {rid}.")]
    public static partial void EndResolvingReference(ILogger logger, ResourceID rid);
}