namespace Caxivitual.Lunacub.Building;

internal static partial class Log {
    [LoggerMessage(LogLevel.Information, "Begin building environment resources.")]
    public static partial void BeginBuildingEnvironmentResources(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Finished building environment resources. Now begin building procedural resources.")]
    public static partial void BeginBuildingProceduralResources(ILogger logger);

    [LoggerMessage(LogLevel.Information, "{amount} procedural resource(s) detected.")]
    public static partial void ProceduralResourcesDetected(ILogger logger, int amount);
    
    [LoggerMessage(LogLevel.Information, "Finish building resources.")]
    public static partial void FinishBuildingResources(ILogger logger);
    
    [LoggerMessage(LogLevel.Information, "Flushing resources' incremental informations...")]
    public static partial void FlushingIncrementalInformations(ILogger logger);
    
    [LoggerMessage(LogLevel.Information, "Step 1: Create resource vertices...")]
    public static partial void CreateVertices(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Step 2: Extract dependencies for modified resources...")]
    public static partial void ExtractVerticesDependenciesForModified(ILogger logger);
    
    [LoggerMessage(LogLevel.Information, "Step 2: Extract dependencies for all resources...")]
    public static partial void ExtractVerticesDependenciesForAll(ILogger logger);
    
    [LoggerMessage(LogLevel.Information, "Step 3: Count vertex references...")]
    public static partial void CountResourceVertexReferences(ILogger logger);
    
    [LoggerMessage(LogLevel.Information, "Step 4: Compile environment resources...")]
    public static partial void CompileEnvironmentResources(ILogger logger);
    
    [LoggerMessage(LogLevel.Information, "Begin building procedural resources layer {layer}.")]
    public static partial void BeginBuildingProceduralResources(ILogger logger, int layer);
    
    [LoggerMessage(LogLevel.Information, "Step 1: Validate dependency graph.")]
    public static partial void ValidatingDependencyGraph(ILogger logger);
    
    [LoggerMessage(LogLevel.Information, "Step 2: Count environment resources reference count.")]
    public static partial void CountEnvironmentResourcesReferenceCount(ILogger logger);
    
    [LoggerMessage(LogLevel.Information, "Step 3: Compile procedural resources...")]
    public static partial void CompileProceduralResources(ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Detected a resource cycle after populating graph vertices from resources incremental informations. A fresh rebuild will be executed.")]
    public static partial void WarnGraphCycleDetectedAfterPopulateVerticesFromIncrementalInfos(ILogger logger);
    
    [LoggerMessage(LogLevel.Warning, "Detected leaked resources after building envirionment resources. Cleaning up everything...")]
    public static partial void ReportLeakedAfterBuildEnvironmentResources(ILogger logger);
}