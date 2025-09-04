namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void ProcessPostBuild() {
        Log.FlushingIncrementalInformations(_environment.Logger);

        foreach ((LibraryID libraryId, ResourceRegistry<ResourceRegistry.Element> registry) in _outputRegistries) {
            // Output the registry.
            _environment.Output.OutputLibraryRegistry(registry, libraryId);
        }
        
        _environment.FlushIncrementalInfos();
    }
}