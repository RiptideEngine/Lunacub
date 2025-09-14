using Caxivitual.Lunacub.Exceptions;
using Caxivitual.Lunacub.Tests.Common;
using System.Diagnostics;

namespace Caxivitual.Lunacub.Tests;

public class SourceRepositoryTests {
    [Fact]
    public void ReturnsNullStream_ShouldNotThrowException() {
        var provider = new NullReturnSourceRepository();
        new Func<Stream?>(() => provider.CreateStream(string.Empty)).Should().NotThrow().Which.Should().BeNull();
    }

    [Fact]
    public void ReturnsUnreadableStream_ShouldThrowException() {
        var provider = new ConfigurableStreamSourceRepository(false, true, true);
        new Func<Stream?>(() => provider.CreateStream(string.Empty))
            .Should().Throw<InvalidResourceStreamException>().WithMessage("*readable*");
    }
    
    [Fact]
    public void ReturnsUnseekableStream_ShouldThrowException() {
        var provider = new ConfigurableStreamSourceRepository(true, false, true);
        new Func<Stream?>(() => provider.CreateStream(string.Empty))
            .Should().Throw<InvalidResourceStreamException>().WithMessage("*seekable*");
    }

    [Fact]
    public void ReturnsWritableStream_ShouldThrowException() {
        var provider = new ConfigurableStreamSourceRepository(true, true, true);
        new Func<Stream?>(() => provider.CreateStream(string.Empty))
            .Should().Throw<InvalidResourceStreamException>().WithMessage("*not*writable*");
    }
    
    private sealed class NullReturnSourceRepository : SourceRepository<string> {
        protected override Stream? CreateStreamCore(string address) {
            return null;
        }
    }

    private sealed class ConfigurableStreamSourceRepository(bool readable, bool seekable, bool writable) : SourceRepository<string> {
        protected override Stream? CreateStreamCore(string address) {
            return new ConfigurableStream(readable, seekable, writable);
        }
    }
}