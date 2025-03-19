using Caxivitual.Lunacub.Building.Serialization;

namespace Caxivitual.Lunacub.Building;

[JsonConverter(typeof(IncrementalInfoConverter))]
public readonly struct IncrementalInfo {
    public readonly DateTime SourceLastWriteTime;
    public readonly BuildingOptions Options;

    public IncrementalInfo(DateTime sourceLastWriteTime, BuildingOptions options) {
        SourceLastWriteTime = sourceLastWriteTime;
        Options = options;
    }
}