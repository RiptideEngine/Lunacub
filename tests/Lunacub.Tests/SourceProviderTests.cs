using Caxivitual.Lunacub.Exceptions;

namespace Caxivitual.Lunacub.Tests;

public class SourceProviderTests {
    [Fact]
    public void ReturnsNullStream_ShouldNotThrowException() {
        SourceProvider provider = new NullReturnSourceProvider();
        new Func<Stream?>(() => provider.CreateStream(string.Empty)).Should().NotThrow().Which.Should().BeNull();
    }

    [Fact]
    public void ReturnsUnreadableStream_ShouldThrowException() {
        SourceProvider provider = new ConfigurableStreamSourceProvider(false, true, true);
        new Func<Stream?>(() => provider.CreateStream(string.Empty))
            .Should().Throw<InvalidResourceStreamException>().WithMessage("*readable*");
    }
    
    [Fact]
    public void ReturnsUnseekableStream_ShouldThrowException() {
        SourceProvider provider = new ConfigurableStreamSourceProvider(true, false, true);
        new Func<Stream?>(() => provider.CreateStream(string.Empty))
            .Should().Throw<InvalidResourceStreamException>().WithMessage("*seekable*");
    }

    [Fact]
    public void ReturnsWritableStream_ShouldThrowException() {
        SourceProvider provider = new ConfigurableStreamSourceProvider(true, true, true);
        new Func<Stream?>(() => provider.CreateStream(string.Empty))
            .Should().Throw<InvalidResourceStreamException>().WithMessage("*not*writable*");
    }
    
    private sealed class NullReturnSourceProvider : SourceProvider {
        protected override Stream? CreateStreamCore(string address) {
            return null;
        }
    }
    
    private sealed class ConfigurableStream : Stream {
        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        
        public override long Length => 0;
        public override long Position { get; set; }

        public ConfigurableStream(bool readable, bool seekable, bool writable) {
            CanRead = readable;
            CanSeek = seekable;
            CanWrite = writable;
        }
        
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => 0;
        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
        public override void SetLength(long value) {
            throw new NotImplementedException();
        }
        public override void Write(byte[] buffer, int offset, int count) { }
    }

    private sealed class ConfigurableStreamSourceProvider(bool readable, bool seekable, bool writable) : SourceProvider {
        protected override Stream? CreateStreamCore(string address) {
            return new ConfigurableStream(readable, seekable, writable);
        }
    }
}