using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Caxivitual.Lunacub.Tests.Importing;

[SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known")]
public class DeserializationContextTests {
    private readonly DeserializationContext _context = new();
    
    [Theory]
    [InlineData(typeof(object))]
    [InlineData(typeof(IEnumerable<int>))]
    [InlineData(typeof(Dictionary<int, string>))]
    public void RequestReference_NonGeneric_RegistersCorrectly(Type type) {
        ReferencePropertyKey key = new(10);
        
#pragma warning disable CA2263
        new Action(() => _context.RequestReference(key, 1, type)).Should().NotThrow();
#pragma warning restore CA2263

        _context.RequestingDependencies.Should().HaveCount(1);
        _context.RequestingDependencies.Should().ContainKey(key).WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(1, type));
    }
    
    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(Quaternion))]
    [InlineData(typeof(Vector4))]
    public void RequestReference_NonGeneric_IgnoreValueType(Type type) {
#pragma warning disable CA2263
        new Action(() => _context.RequestReference(new(10), 1, type)).Should().NotThrow();
#pragma warning restore CA2263

        _context.RequestingDependencies.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(typeof(Dictionary<,>))]
    [InlineData(typeof(List<>))]
    public void RequestReference_NonGeneric_IgnoreGenericTypeDefinintion(Type type) {
#pragma warning disable CA2263
        new Action(() => _context.RequestReference(new(10), 1, type)).Should().NotThrow();
#pragma warning restore CA2263

        _context.RequestingDependencies.Should().BeEmpty();
    }
    
    [Fact]
    public void RequestReference_NonGeneric_IgnoreNullResourceID() {
        new Action(() => _context.RequestReference(new(10), ResourceID.Null, typeof(object))).Should().NotThrow();
        
        _context.RequestingDependencies.Should().BeEmpty();
    }
    
    [Fact]
    public void RequestReferenceGeneric_RegistersCorrectly() {
        ReferencePropertyKey property1 = new(1);
        ReferencePropertyKey property2 = new(2);
        ReferencePropertyKey property3 = new(3);
        
        new Action(() => _context.RequestReference<object>(property1, 1)).Should().NotThrow();
        new Action(() => _context.RequestReference<IEnumerable<int>>(property2, 2)).Should().NotThrow();
        new Action(() => _context.RequestReference<List<int>>(property3, 3)).Should().NotThrow();

        _context.RequestingDependencies.Should().HaveCount(3);
        _context.RequestingDependencies.Should().ContainKey(property1).WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(1, typeof(object)));
        _context.RequestingDependencies.Should().ContainKey(property2).WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(2, typeof(IEnumerable<int>)));
        _context.RequestingDependencies.Should().ContainKey(property3).WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(3, typeof(List<int>)));
    }
    
    [Fact]
    public void RequestReference_Generic_IgnoreNullResourceID() {
        new Action(() => _context.RequestReference<object>(new(10), ResourceID.Null)).Should().NotThrow();
        
        _context.RequestingDependencies.Should().BeEmpty();
    }
}