namespace Caxivitual.Lunacub.Tests;

public class ResourceHandleTests {
    [Fact]
    public void NonGenericConvert_Downcast_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Parent>>(() => new ResourceHandle(ResourceID.Null, new Child1()).Convert<Parent>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }

    [Fact]
    public void NonGenericConvert_CorrectType_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Child1>>(() => new ResourceHandle(ResourceID.Null, new Child1()).Convert<Child1>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }
    
    [Fact]
    public void NonGenericConvert_IncorrectType_ShouldNotThrow() {
        new Func<ResourceHandle<Child2>>(() => new ResourceHandle(ResourceID.Null, new Child1()).Convert<Child2>())
            .Should().NotThrow()
            .Which.Value
            .Should().BeNull();
    }
    
    [Fact]
    public void NonGenericConvertUnsafe_Downcast_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Parent>>(() => new ResourceHandle(ResourceID.Null, new Child1()).ConvertUnsafe<Parent>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }

    [Fact]
    public void NonGenericConvertUnsafe_CorrectType_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Child1>>(() => new ResourceHandle(ResourceID.Null, new Child1()).ConvertUnsafe<Child1>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }
    
    [Fact]
    public void NonGenericConvertUnsafe_IncorrectType_ThrowsInvalidCastException() {
        new Func<ResourceHandle<Child2>>(() => new ResourceHandle(ResourceID.Null, new Child1()).ConvertUnsafe<Child2>())
            .Should().Throw<InvalidCastException>();
    }
    
    [Fact]
    public void GenericToNonGeneric_ImplicitConvert_ReturnsObjectHandle() {
        new Func<ResourceHandle>(() => new ResourceHandle<Child1>(ResourceID.Null, new()))
            .Should().NotThrow()
            .Which.Value
            .Should().BeOfType<Child1>();
    }
    
    [Fact]
    public void GenericConvert_Downcast_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Parent>>(() => new ResourceHandle<Child1>(ResourceID.Null, new()).Convert<Parent>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }

    [Fact]
    public void GenericConvert_UpcastCorrectType_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Child1>>(() => new ResourceHandle<Parent>(ResourceID.Null, new Child1()).Convert<Child1>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }
    
    [Fact]
    public void GenericConvert_UpcastIncorrectType_ShouldNotThrow() {
        new Func<ResourceHandle<Child2>>(() => new ResourceHandle<Parent>(ResourceID.Null, new Child1()).Convert<Child2>())
            .Should().NotThrow()
            .Which.Value
            .Should().BeNull();
    }
    
    [Fact]
    public void GenericConvertUnsafe_Downcast_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Parent>>(() => new ResourceHandle<Child1>(ResourceID.Null, new()).ConvertUnsafe<Parent>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }

    [Fact]
    public void GenericConvertUnsafe_UpcastCorrectType_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Child1>>(() => new ResourceHandle<Parent>(ResourceID.Null, new Child1()).ConvertUnsafe<Child1>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }
    
    [Fact]
    public void GenericConvertUnsafe_UpcastIncorrectType_ThrowsInvalidCastException() {
        new Func<ResourceHandle<Child2>>(() => new ResourceHandle<Parent>(ResourceID.Null, new Child1()).ConvertUnsafe<Child2>())
            .Should().Throw<InvalidCastException>();
    }

    private record Parent;
    private record Child1 : Parent;
    private record Child2 : Parent;
}