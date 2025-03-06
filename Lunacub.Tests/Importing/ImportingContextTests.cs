namespace Caxivitual.Lunacub.Tests.Importing;

public sealed class ImportingContextTests {
    private readonly ImportingContext _context;
    private readonly ITestOutputHelper _output;

    public ImportingContextTests(ITestOutputHelper output) {
        _output = output;
        AssertHelpers.RedirectConsoleOutput(output);        
        
        string outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
        string reportDirectory = Path.Combine(outputDirectory, "Report");
        string resultDirectory = Path.Combine(outputDirectory, "Results");
        
        if (Directory.Exists(outputDirectory)) {
            Directory.Delete(outputDirectory, true);
        };
        
        Directory.CreateDirectory(reportDirectory);
        Directory.CreateDirectory(resultDirectory);
    }
}