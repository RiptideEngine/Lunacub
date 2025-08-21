using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing.Extensions;

public static class ImportEnvironmentExtensions {
    [OverloadResolutionPriority(1)]
    public static ImportEnvironment SetDeserializer(this ImportEnvironment environment, string name, Deserializer deserializer) {
        environment.Deserializers[name] = deserializer;
        return environment;
    }

    public static ImportEnvironment SetDeserializer(this ImportEnvironment environment, ReadOnlySpan<char> name, Deserializer deserializer) {
        environment.Deserializers[name] = deserializer;
        return environment;
    }
    
    public static ImportEnvironment AddDisposer(this ImportEnvironment environment, Disposer disposer) {
        environment.Disposers.Add(disposer);
        return environment;
    }
    
    public static ImportEnvironment SetLogger(this ImportEnvironment environment, ILogger logger) {
        environment.Logger = logger;
        return environment;
    }

    public static ImportEnvironment AddLibrary(this ImportEnvironment environment, ImportResourceLibrary library) {
        environment.Libraries.Add(library);
        return environment;
    }
}