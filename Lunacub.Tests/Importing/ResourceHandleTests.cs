namespace Caxivitual.Lunacub.Tests.Importing;

public class ResourceHandleTests {
    [Fact]
    public void NonGenericToGenericConvert_ShouldBeCorrect() {
        ResourceHandle handle = new(ResourceID.Null, new Child1());
        
        new Func<ResourceHandle<Child1>>(() => handle.Convert<Child1>()).Should().NotThrow().Which.Value.Should().NotBeNull();
        new Func<ResourceHandle<Child1>>(() => handle.ConvertUnchecked<Child1>()).Should().NotThrow().Which.Value.Should().NotBeNull();

        new Func<ResourceHandle<Child2>>(() => handle.Convert<Child2>()).Should().Throw<InvalidCastException>();
        new Func<ResourceHandle<Child2>>(() => handle.ConvertUnchecked<Child2>()).Should().NotThrow().Which.Value.Should().BeNull();
    }
    
    [Fact]
    public void GenericToNonGenericConvert_ShouldBeCorrect() {
        ResourceHandle<Child1> handle = new(ResourceID.Null, new());
        
        new Func<ResourceHandle>(() => handle).Should().NotThrow().Which.Value.Should().NotBeNull();
    }
    
    [Fact]
    public void Generic_DownwardConvert_ShouldBeCorrect() {
        ResourceHandle<Child1> handle = new(ResourceID.Null, new());

        new Func<ResourceHandle<Parent1>>(() => handle.Convert<Parent1>()).Should().NotThrow().Which.Value.Should().NotBeNull();
        new Func<ResourceHandle<Parent1>>(() => handle.ConvertUnchecked<Parent1>()).Should().NotThrow().Which.Value.Should().NotBeNull();
        
        new Func<ResourceHandle<Parent2>>(() => handle.Convert<Parent2>()).Should().ThrowExactly<InvalidCastException>();
        new Func<ResourceHandle<Parent2>>(() => handle.ConvertUnchecked<Parent2>()).Should().NotThrow().Which.Value.Should().BeNull();
    }
    
    [Fact]
    public void Generic_UpwardConvert_ShouldBeCorrect() {
        ResourceHandle<Parent1> handle = new(ResourceID.Null, new Child1());
        new Func<ResourceHandle<Child1>>(() => handle.Convert<Child1>()).Should().NotThrow().Which.Value.Should().NotBeNull();
        new Func<ResourceHandle<Child1>>(() => handle.ConvertUnchecked<Child1>()).Should().NotThrow().Which.Value.Should().NotBeNull();
        
        new Func<ResourceHandle<Child2>>(() => handle.Convert<Child2>()).Should().ThrowExactly<InvalidCastException>();
        new Func<ResourceHandle<Child2>>(() => handle.ConvertUnchecked<Child2>()).Should().NotThrow().Which.Value.Should().BeNull();
    }

    private abstract class Parent1;
    private sealed class Child1 : Parent1;
    
    private abstract class Parent2;
    private sealed class Child2 : Parent2;
}