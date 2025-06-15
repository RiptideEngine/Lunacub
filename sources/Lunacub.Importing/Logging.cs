using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

[ExcludeFromCodeCoverage]
internal static partial class Logging {
    // public static EventId BeginImportEvent => new(1, "BeginImport");
    // public static EventId ImportExceptionOccuredEvent => new(2, "ImportExceptionOccured");
    // public static EventId DependencyImportExceptionOccuredEvent => new(3, "DependencyImportExceptionOccured");
    // public static EventId ImportCancelEvent => new(4, "ImportCancel");
    // public static EventId ImportUnregisteredDependencyEvent => new(5, "ImportUnregisteredDependency");
    // public static EventId ResolveDependenciesEvent => new(6, "ResolveDependencies");

    [LoggerMessage(LogLevel.Information, "Begin import resource {rid}.")]
    public static partial void BeginImport(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Information, "Begin import reference resource {rid}.")]
    public static partial void BeginImportReference(ILogger logger, ResourceID rid);

    [LoggerMessage(LogLevel.Warning, "Trying to import an unregistered resource {rid}.")]
    public static partial void UnregisteredResource(ILogger logger, ResourceID rid);

    [LoggerMessage(LogLevel.Warning, "Trying to import an unregistered dependency resource {rid}.")]
    public static partial void UnregisteredDependencyResource(ILogger logger, ResourceID rid);
}