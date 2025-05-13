using System.Numerics;

namespace Caxivitual.Lunacub.Tests.Importing;

public class DeserializationContextTests {
    private readonly DeserializationContext _context = new();
    
    [Theory]
    [InlineData(typeof(object))]
    [InlineData(typeof(IEnumerable<int>))]
    [InlineData(typeof(Dictionary<int, string>))]
    public void RequestReference_NonGeneric_RegistersCorrectly(Type type) {
#pragma warning disable CA2263
        new Action(() => _context.RequestReference("Property", 1, type)).Should().NotThrow();
#pragma warning restore CA2263

        _context.RequestingDependencies.Should().HaveCount(1);
        _context.RequestingDependencies.Should().ContainKey("Property").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(1, type));
    }
    
    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(Quaternion))]
    [InlineData(typeof(Vector4))]
    public void RequestReference_NonGeneric_IgnoreValueType(Type type) {
#pragma warning disable CA2263
        new Action(() => _context.RequestReference("Property", 1, type)).Should().NotThrow();
#pragma warning restore CA2263

        _context.RequestingDependencies.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(typeof(Dictionary<,>))]
    [InlineData(typeof(List<>))]
    public void RequestReference_NonGeneric_IgnoreGenericTypeDefinintion(Type type) {
#pragma warning disable CA2263
        new Action(() => _context.RequestReference("Property", 1, type)).Should().NotThrow();
#pragma warning restore CA2263

        _context.RequestingDependencies.Should().BeEmpty();
    }
    
    [Fact]
    public void RequestReference_NonGeneric_IgnoreNullResourceID() {
        new Action(() => _context.RequestReference("Property", ResourceID.Null, typeof(object))).Should().NotThrow();
        
        _context.RequestingDependencies.Should().BeEmpty();
    }
    
    [Fact]
    public void RequestReferenceGeneric_RegistersCorrectly() {
        new Action(() => _context.RequestReference<object>("Property1", 1)).Should().NotThrow();
        new Action(() => _context.RequestReference<IEnumerable<int>>("Property2", 2)).Should().NotThrow();
        new Action(() => _context.RequestReference<List<int>>("Property3", 3)).Should().NotThrow();

        _context.RequestingDependencies.Should().HaveCount(3);
        _context.RequestingDependencies.Should().ContainKey("Property1").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(1, typeof(object)));
        _context.RequestingDependencies.Should().ContainKey("Property2").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(2, typeof(IEnumerable<int>)));
        _context.RequestingDependencies.Should().ContainKey("Property3").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(3, typeof(List<int>)));
    }
    
    [Fact]
    public void RequestReference_Generic_IgnoreNullResourceID() {
        new Action(() => _context.RequestReference<object>("Property1", ResourceID.Null)).Should().NotThrow();
        
        _context.RequestingDependencies.Should().BeEmpty();
    }
}