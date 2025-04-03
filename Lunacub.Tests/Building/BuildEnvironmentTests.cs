namespace Caxivitual.Lunacub.Tests.Building;

public partial class BuildEnvironmentTests : IDisposable {
    private readonly BuildEnvironment _context;
    private readonly ITestOutputHelper _output;

    public BuildEnvironmentTests(ITestOutputHelper output) {
        _output = output;
        AssertHelpers.RedirectConsoleOutput(output);

        _context = new(new MockOutputSystem());
        
        _context.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
        _context.SerializerFactories.Add(new SimpleResourceSerializerFactory());
        
        _context.Importers.Add(nameof(ReferenceResourceImporter), new ReferenceResourceImporter());
        _context.SerializerFactories.Add(new ReferenceResourceSerializerFactory());
        
        _context.Importers.Add(nameof(OptionsResourceImporter), new OptionsResourceImporter());
        _context.Processors.Add(nameof(OptionsResourceProcessor), new OptionsResourceProcessor());
        _context.SerializerFactories.Add(new OptionsResourceSerializerFactory());
    }

    public void Dispose() {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string GetResourcePath(string filename) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filename);
}