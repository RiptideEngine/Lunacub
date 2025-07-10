using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

[ExcludeFromCodeCoverage]
internal static partial class Log {
    [LoggerMessage(LogLevel.Information, "Begin import resource {rid}.")]
    public static partial void BeginImport(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Information, "Begin resolving references for resource {rid}.")]
    public static partial void BeginResolvingReference(ILogger logger, ResourceID rid);
    
    [LoggerMessage(LogLevel.Information, "End resolving references for resource {rid}.")]
    public static partial void EndResolvingReference(ILogger logger, ResourceID rid);
}