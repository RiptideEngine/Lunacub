using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

public sealed class DeserializationContext {
    public RequestingReferences RequestingReferences { get; }
    public ILogger Logger { get; }
    public Dictionary<object, object> ValueContainer { get; }

    internal DeserializationContext(ILogger logger) {
        Logger = logger;
        RequestingReferences = new();
        ValueContainer = [];
    }
}