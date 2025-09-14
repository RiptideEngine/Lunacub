using System.Diagnostics;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class ConfigurableStream : Stream {
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
    public override long Seek(long offset, SeekOrigin origin) => throw new UnreachableException();
    public override void SetLength(long value) {
        throw new UnreachableException();
    }
    public override void Write(byte[] buffer, int offset, int count) { }
}