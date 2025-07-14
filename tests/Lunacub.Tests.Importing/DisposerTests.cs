namespace Caxivitual.Lunacub.Tests.Importing;

public class DisposerTests {
    [Fact]
    public void TryDispose_DelegateDisposer_ShouldDisposeCorrectObjectAndReturnsCorrectResult() {
        DisposableResource resource = new();
        
        Disposer.Create(obj => {
            if (obj is not IDisposable disposable) return false;

            disposable.Dispose();
            return true;
        }).TryDispose(resource).Should().BeTrue();
        
        resource.Disposed.Should().BeTrue();
    }
}