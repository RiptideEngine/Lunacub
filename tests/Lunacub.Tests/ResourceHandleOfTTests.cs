namespace Caxivitual.Lunacub.Tests;

public class ResourceHandleOfTTests {
    [Fact]
    public void GenericToNonGeneric_ImplicitConvert_ReturnsObjectHandle() {
        new Func<ResourceHandle>(() => new ResourceHandle<Child1>(default, new()))
            .Should().NotThrow()
            .Which.Value
            .Should().BeOfType<Child1>();
    }
    
    [Fact]
    public void Convert_Downcast_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Parent>>(() => new ResourceHandle<Child1>(default, new()).Convert<Parent>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }

    [Fact]
    public void Convert_UpcastCorrectType_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Child1>>(() => new ResourceHandle<Parent>(default, new Child1()).Convert<Child1>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }
    
    [Fact]
    public void Convert_UpcastIncorrectType_ShouldNotThrow() {
        new Func<ResourceHandle<Child2>>(() => new ResourceHandle<Parent>(default, new Child1()).Convert<Child2>())
            .Should().NotThrow()
            .Which.Value
            .Should().BeNull();
    }
    
    [Fact]
    public void ConvertUnsafe_Downcast_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Parent>>(() => new ResourceHandle<Child1>(default, new()).ConvertUnsafe<Parent>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }

    [Fact]
    public void ConvertUnsafe_UpcastCorrectType_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Child1>>(() => new ResourceHandle<Parent>(default, new Child1()).ConvertUnsafe<Child1>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }
    
    [Fact]
    public void ConvertUnsafe_UpcastIncorrectType_ThrowsInvalidCastException() {
        new Func<ResourceHandle<Child2>>(() => new ResourceHandle<Parent>(default, new Child1()).ConvertUnsafe<Child2>())
            .Should().Throw<InvalidCastException>();
    }
    
    [Fact]
    public void Deconstruct_ShouldOutputCorrectly() {
        (ResourceAddress address, object? child2) = new ResourceHandle<Child2>(new(255, 255), new());

        address.Should().Be(new ResourceAddress(255, 255));
        child2.Should().BeOfType<Child2>();
    }
    
    private record Parent;
    private record Child1 : Parent;
    private record Child2 : Parent;
}