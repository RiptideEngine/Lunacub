using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

[ExcludeFromCodeCoverage]
internal static partial class Log {
    [LoggerMessage(LogLevel.Information, "Begin import resource L{libraryId}-R{resourceId}.")]
    public static partial void BeginImport(ILogger logger, LibraryID libraryId, ResourceID resourceId);
    
    [LoggerMessage(LogLevel.Information, "Cancelled importing resource L{libraryId}-R{resourceId}.")]
    public static partial void CancelImport(ILogger logger, LibraryID libraryId, ResourceID resourceId);
    
    [LoggerMessage(LogLevel.Information, "Begin resolving references for resource L{libraryId}-R{resourceId}.")]
    public static partial void BeginResolvingReference(ILogger logger, LibraryID libraryId, ResourceID resourceId);
    
    [LoggerMessage(LogLevel.Information, "End resolving references for resource L{libraryId}-R{resourceId}.")]
    public static partial void EndResolvingReference(ILogger logger, LibraryID libraryId, ResourceID resourceId);
    
    [LoggerMessage(LogLevel.Error, "Exception occured while importing resource L{libraryId}-R{resourceId}.")]
    public static partial void ReportImportException(ILogger logger, LibraryID libraryId, ResourceID resourceId, Exception ex);

    [LoggerMessage(LogLevel.Error, "Exception occured while waiting import task of resource L{libraryId}-R{resourceId}.")]
    public static partial void ResolveReferenceExceptionOccured(ILogger logger, LibraryID libraryId, ResourceID resourceId);
    
    [LoggerMessage(LogLevel.Error, "Exception occured while waiting reference resolve task of resource L{libraryId}-R{resourceId}.")]
    public static partial void FinalizeTaskExceptionOccured(ILogger logger, LibraryID libraryId, ResourceID resourceId);
}