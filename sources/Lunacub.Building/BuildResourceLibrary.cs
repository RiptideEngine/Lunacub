using Caxivitual.Lunacub.Exceptions;
using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

public sealed class BuildResourceLibrary {
    private readonly BuildSourceProvider _provider;

    public ResourceRegistry<ResourceRegistry.Element<BuildingResource>> Registry { get; }
    
    public BuildResourceLibrary(BuildSourceProvider sourceProvider) {
        _provider = sourceProvider;
        Registry = [];
    }

    public SourceStreams CreateSourceStreams(ResourceID resourceId) {
        if (!Registry.TryGetValue(resourceId, out ResourceRegistry.Element<BuildingResource> element)) {
            return new(null, FrozenDictionary<string, Stream?>.Empty);
        }

        SourceAddresses addresses = element.Option.Addresses;

        Stream? primaryStream = _provider.CreateStream(addresses.Primary);

        try {
            int secondaryStreamsCount = addresses.Secondaries.Count;
            
            if (secondaryStreamsCount > 0) {
                Dictionary<string, Stream?> secondaryStreams = new(secondaryStreamsCount);

                try {
                    foreach ((var name, var address) in addresses.Secondaries) {
                        secondaryStreams.Add(name, _provider.CreateStream(address));
                    }
                } catch {
                    foreach (var stream in secondaryStreams.Values) {
                        stream?.Dispose();
                    }
                    
                    throw;
                }

                return new(primaryStream, secondaryStreams);
            }
            
            return new(primaryStream, FrozenDictionary<string, Stream?>.Empty);
        } catch {
            primaryStream?.Dispose();
            throw;
        }
    }

    public SourceLastWriteTimes GetSourceLastWriteTimes(ResourceID resourceId) {
        if (!Registry.TryGetValue(resourceId, out ResourceRegistry.Element<BuildingResource> element)) {
            return new(DateTime.MinValue, FrozenDictionary<string, DateTime>.Empty);
        }
        
        var sourceAddresses = element.Option.Addresses;

        DateTime primaryLastWriteTime = _provider.GetLastWriteTime(sourceAddresses.Primary);

        if (sourceAddresses.Secondaries.Count > 0) {
            Dictionary<string, DateTime> secondaries = [];

            foreach ((var name, var address) in sourceAddresses.Secondaries) {
                secondaries.Add(name, _provider.GetLastWriteTime(address));
            }
            
            return new(primaryLastWriteTime, secondaries);
        }

        return new(primaryLastWriteTime);
    }
}