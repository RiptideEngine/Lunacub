namespace Caxivitual.Lunacub.Building.Extensions;

public static class BuildEnvironmentExtensions {
    [OverloadResolutionPriority(1)]
    public static BuildEnvironment SetImporter(this BuildEnvironment environment, string name, Importer importer) {
        environment.Importers.Add(name, importer);
        return environment;
    }
    
    public static BuildEnvironment SetImporter(this BuildEnvironment environment, ReadOnlySpan<char> name, Importer importer) {
        environment.Importers[name] = importer;
        return environment;
    }
    
    [OverloadResolutionPriority(1)]
    public static BuildEnvironment SetProcessor(this BuildEnvironment environment, string name, Processor Processor) {
        environment.Processors.Add(name, Processor);
        return environment;
    }
    
    public static BuildEnvironment SetProcessor(this BuildEnvironment environment, ReadOnlySpan<char> name, Processor Processor) {
        environment.Processors[name] = Processor;
        return environment;
    }

    public static BuildEnvironment AddSerializerFactory(this BuildEnvironment environment, SerializerFactory factory) {
        environment.SerializerFactories.Add(factory);
        return environment;
    }

    public static BuildEnvironment SetLogger(this BuildEnvironment environment, ILogger logger) {
        environment.Logger = logger;
        return environment;
    }

    public static BuildEnvironment AddLibrary(this BuildEnvironment environment, BuildResourceLibrary library) {
        environment.Libraries.Add(library);
        return environment;
    }
}