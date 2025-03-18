﻿namespace Caxivitual.Lunacub.Tests.Building;

public partial class BuildEnvironmentTests : IDisposable {
    private readonly BuildEnvironment _context;
    private readonly ITestOutputHelper _output;

    public BuildEnvironmentTests(ITestOutputHelper output) {
        _output = output;
        AssertHelpers.RedirectConsoleOutput(output);

        _context = new(new MockOutputSystem());
        
        _context.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
        _context.Serializers.Add(new SimpleResourceSerializer());
        
        _context.Importers.Add(nameof(DependentResourceImporter), new DependentResourceImporter());
        _context.Serializers.Add(new DependentResourceSerializer());
        
        _context.Importers.Add(nameof(CircularReferenceResource), new CircularReferenceResourceImporter());
        _context.Serializers.Add(new CircularReferenceResourceSerializer());
    }

    public void Dispose() {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string GetResourcePath(string filename) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filename);
}