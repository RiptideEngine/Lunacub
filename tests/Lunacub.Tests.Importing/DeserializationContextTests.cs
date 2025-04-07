using System.Numerics;

namespace Caxivitual.Lunacub.Tests.Importing;

public class DeserializationContextTests {
    [Fact]
    public void DeserializationContext_RequestReference_ShouldBeCorrect() {
        DeserializationContext context = new();

        ResourceID rid1 = ResourceID.Parse("2bb9d21d128f11f090eb089798ecb32c");
        ResourceID rid2 = ResourceID.Parse("9d5ba8cf128f11f090eb089798ecb32c");
        ResourceID rid3 = ResourceID.Parse("089a2be2129011f090eb089798ecb32c");
        
        new Action(() => context.RequestReference("Property1", rid1, typeof(object))).Should().NotThrow();
        new Action(() => context.RequestReference("Property2", rid2, typeof(IEnumerable<int>))).Should().NotThrow();
        new Action(() => context.RequestReference("Property3", rid3, typeof(List<int>))).Should().NotThrow();

        context.RequestingDependencies.Should().HaveCount(3);
        context.RequestingDependencies.Should().ContainKey("Property1").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid1, typeof(object)));
        context.RequestingDependencies.Should().ContainKey("Property2").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid2, typeof(IEnumerable<int>)));
        context.RequestingDependencies.Should().ContainKey("Property3").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid3, typeof(List<int>)));
    }
    
    [Fact]
    public void DeserializationContext_RequestReference_Generic_ShouldBeCorrect() {
        DeserializationContext context = new();

        ResourceID rid1 = ResourceID.Parse("2bb9d21d128f11f090eb089798ecb32c");
        ResourceID rid2 = ResourceID.Parse("9d5ba8cf128f11f090eb089798ecb32c");
        ResourceID rid3 = ResourceID.Parse("089a2be2129011f090eb089798ecb32c");
        
        new Action(() => context.RequestReference<object>("Property1", rid1)).Should().NotThrow();
        new Action(() => context.RequestReference<IEnumerable<int>>("Property2", rid2)).Should().NotThrow();
        new Action(() => context.RequestReference<List<int>>("Property3", rid3)).Should().NotThrow();

        context.RequestingDependencies.Should().HaveCount(3);
        context.RequestingDependencies.Should().ContainKey("Property1").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid1, typeof(object)));
        context.RequestingDependencies.Should().ContainKey("Property2").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid2, typeof(IEnumerable<int>)));
        context.RequestingDependencies.Should().ContainKey("Property3").WhoseValue.Should().Be(new DeserializationContext.RequestingDependency(rid3, typeof(List<int>)));
    }
    
    [Fact]
    public void DeserializationContext_RequestReference_ShouldNotAllowStruct() {
        DeserializationContext context = new();

        new Action(() => context.RequestReference<Vector4>("Property1", ResourceID.Null)).Should().NotThrow();

        context.RequestingDependencies.Should().BeEmpty();
    }
}