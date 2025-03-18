namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void CircularDependentResource_ShouldBeCorrect() {
        var cdr1 = ResourceID.Parse("0195921b2ac17986b78abcb187f64dd2");
        var cdr2 = ResourceID.Parse("0195921aa48a7153906e0ecd9a7bdf33");
        
        _context.Resources.Add(cdr1, GetResourcePath("CircularReferenceResource1.json"), new() {
            ImporterName = nameof(CircularReferenceResource),
            ProcessorName = null,
        });
        _context.Resources.Add(cdr2, GetResourcePath("CircularReferenceResource2.json"), new() {
            ImporterName = nameof(CircularReferenceResource),
            ProcessorName = null,
        });
    
        var report = _context.BuildResources();
        report.Exception.Should().BeNull();
    }
}