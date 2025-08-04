namespace Caxivitual.Lunacub.Tests;

public class ResourceHandleTests {
    [Fact]
    public void Convert_Downcast_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Parent>>(() => new ResourceHandle(default, new Child1()).Convert<Parent>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }

    [Fact]
    public void Convert_CorrectType_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Child1>>(() => new ResourceHandle(default, new Child1()).Convert<Child1>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }
    
    [Fact]
    public void Convert_IncorrectType_ShouldNotThrow() {
        new Func<ResourceHandle<Child2>>(() => new ResourceHandle(default, new Child1()).Convert<Child2>())
            .Should().NotThrow()
            .Which.Value
            .Should().BeNull();
    }
    
    [Fact]
    public void ConvertUnsafe_Downcast_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Parent>>(() => new ResourceHandle(default, new Child1()).ConvertUnsafe<Parent>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }

    [Fact]
    public void ConvertUnsafe_CorrectType_ReturnsCorrectHandle() {
        new Func<ResourceHandle<Child1>>(() => new ResourceHandle(default, new Child1()).ConvertUnsafe<Child1>())
            .Should().NotThrow()
            .Which.Value
            .Should().NotBeNull();
    }
    
    [Fact]
    public void ConvertUnsafe_IncorrectType_ThrowsInvalidCastException() {
        new Func<ResourceHandle<Child2>>(() => new ResourceHandle(default, new Child1()).ConvertUnsafe<Child2>())
            .Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void Deconstruct_ShouldOutputCorrectly() {
        (ResourceAddress address, object? child2) = new ResourceHandle(new(255, 255), new Child2());

        address.Should().Be(new ResourceAddress(255, 255));
        child2.Should().BeOfType<Child2>();
    }

    private record Parent;
    private record Child1 : Parent;
    private record Child2 : Parent;
}