using System.Numerics;

namespace Caxivitual.Lunacub.Tests.Importing;

public class DeserializationContextTests {
    private readonly DeserializationContext _context = new();
    
    [Fact]
    public void RequestReference_RegistersCorrectly() {
        ResourceID rid1 = new("2bb9d21d128f11f090eb089798ecb32c");
        ResourceID rid2 = new("9d5ba8cf128f11f090eb089798ecb32c");
        ResourceID rid3 = new("089a2be2129011f090eb089798ecb32c");
        
#pragma warning disable CA2263
        new Action(() => _context.RequestReference("Property1", rid1, typeof(object))).Should().NotThrow();
        new Action(() => _context.RequestReference("Property2", rid2, typeof(IEnumerable<int>))).Should().NotThrow();
        new Action(() => _context.RequestReference("Property3", rid3, typeof(List<int>))).Should().NotThrow();
#pragma warning restore CA2263

        _context.RequestingDependencies.Should().HaveCount(3);
        _context.RequestingDependencies.Should().ContainKey("Property1").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid1, typeof(object)));
        _context.RequestingDependencies.Should().ContainKey("Property2").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid2, typeof(IEnumerable<int>)));
        _context.RequestingDependencies.Should().ContainKey("Property3").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid3, typeof(List<int>)));
    }
    
    [Fact]
    public void RequestReferenceGeneric_RegistersCorrectly() {
        ResourceID rid1 = new("2bb9d21d128f11f090eb089798ecb32c");
        ResourceID rid2 = new("9d5ba8cf128f11f090eb089798ecb32c");
        ResourceID rid3 = new("089a2be2129011f090eb089798ecb32c");
        
        new Action(() => _context.RequestReference<object>("Property1", rid1)).Should().NotThrow();
        new Action(() => _context.RequestReference<IEnumerable<int>>("Property2", rid2)).Should().NotThrow();
        new Action(() => _context.RequestReference<List<int>>("Property3", rid3)).Should().NotThrow();

        _context.RequestingDependencies.Should().HaveCount(3);
        _context.RequestingDependencies.Should().ContainKey("Property1").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid1, typeof(object)));
        _context.RequestingDependencies.Should().ContainKey("Property2").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid2, typeof(IEnumerable<int>)));
        _context.RequestingDependencies.Should().ContainKey("Property3").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid3, typeof(List<int>)));
    }
    
    [Fact]
    public void RequestReference_ShouldNotAllowStruct() {
        new Action(() => _context.RequestReference<Vector4>("Property1", new("9879400e10b251c5ba7e19a0848e3e60"))).Should().NotThrow();
        _context.RequestingDependencies.Should().BeEmpty();
    }

    [Fact]
    public void RequestReference_ShouldNotRecordNullResourceID() {
        new Action(() => _context.RequestReference<Vector4>("Property1", ResourceID.Null)).Should().NotThrow();
        _context.RequestingDependencies.Should().BeEmpty();
    }
}