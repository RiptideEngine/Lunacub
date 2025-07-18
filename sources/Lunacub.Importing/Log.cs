using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

[ExcludeFromCodeCoverage]
internal static partial class Log {
    [LoggerMessage(LogLevel.Information, "Begin import resource {rid}.")]
    public static partial void BeginImport(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Information, "Cancelled importing resource {rid}.")]
    public static partial void CancelImport(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Information, "Begin resolving references for resource {rid}.")]
    public static partial void BeginResolvingReference(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Information, "End resolving references for resource {rid}.")]
    public static partial void EndResolvingReference(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Debug, "Exception occured while importing resource {rid}.")]
    public static partial void ReportImportException(ILogger logger, ResourceID rid, Exception ex);

    [LoggerMessage(LogLevel.Debug, "Exception occured while waiting import task of resource {rid}.")]
    public static partial void ResolveReferenceExceptionOccured(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Debug, "Exception occured while waiting reference resolve task of resource {rid}.")]
    public static partial void FinalizeTaskExceptionOccured(ILogger logger, ResourceID rid);
}