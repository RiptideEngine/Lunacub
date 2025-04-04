﻿namespace Caxivitual.Lunacub.Tests.Importing;

partial class ImportEnvironmentTests {
    [Fact]
    public void Lifecycle_Import_ShouldBeSuccess() {
        ResourceID rid = ResourceID.Parse("2b786332e04a5874b2499ae7b84bd664");

        BuildResources(rid);

        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        
        var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Value.Should().BeOfType<DisposableResource>();
    }
    
    [Fact]
    public void Lifecycle_DisposeCorrect_ShouldBeSuccess() {
        ResourceID rid = ResourceID.Parse("2b786332e04a5874b2499ae7b84bd664");
        
        BuildResources(rid);

        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        _importEnv.Disposers.Add(Disposer.Create(obj => {
            if (obj is not IDisposable disposable) return false;

            disposable.Dispose();
            return true;
        }));
        
        var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        var resource = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Value.Should().BeOfType<DisposableResource>().Which;
        resource.Disposed.Should().BeFalse();

        new Func<ReleaseStatus>(() => _importEnv.Release(rid)).Should().NotThrow().Which.Should().Be(ReleaseStatus.Success);
        resource.Disposed.Should().BeTrue();
    }
    
    [Fact]
    public void Lifecycle_DisposeNoDisposer_ShouldBeSuccess() {
        ResourceID rid = ResourceID.Parse("2b786332e04a5874b2499ae7b84bd664");
        
        BuildResources(rid);

        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        
        var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        var resource = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Value.Should().BeOfType<DisposableResource>().Which;
        resource.Disposed.Should().BeFalse();

        new Func<ReleaseStatus>(() => _importEnv.Release(rid)).Should().NotThrow().Which.Should().Be(ReleaseStatus.NotDisposed);
        resource.Disposed.Should().BeFalse();
    }

    [Fact]
    public void Lifecycle_ReferenceCount_ShouldBeCorrect() {
        ResourceID rid = ResourceID.Parse("2b786332e04a5874b2499ae7b84bd664");
        
        BuildResources(rid);

        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        _importEnv.Disposers.Add(Disposer.Create(obj => {
            if (obj is not IDisposable disposable) return false;

            disposable.Dispose();
            return true;
        }));
        
        var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        var resource = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Value.Should().BeOfType<DisposableResource>().Which;
        resource.Disposed.Should().BeFalse();
        
        var reference = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which.Value.Should().BeOfType<DisposableResource>().Which;
        reference.Disposed.Should().BeFalse();

        resource.Should().BeSameAs(reference);

        new Func<ReleaseStatus>(() => _importEnv.Release(rid)).Should().NotThrow().Which.Should().Be(ReleaseStatus.Success);
        resource.Disposed.Should().BeFalse();
        
        new Func<ReleaseStatus>(() => _importEnv.Release(rid)).Should().NotThrow().Which.Should().Be(ReleaseStatus.Success);
        resource.Disposed.Should().BeTrue();
    }
}