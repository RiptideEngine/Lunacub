namespace Caxivitual.Lunacub.Importing;

internal sealed class PartialReadStream : Stream {
    private Stream _baseStream;
    private readonly bool _ownStream;
    private readonly long _basePosition;
    private readonly long _length;

    public override long Position {
        get => long.Max(_baseStream.Position - _basePosition, 0);
        set => _baseStream.Position = value + _basePosition;
    }

    public override long Length => _length;

    public override bool CanRead => true;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => false;

    public PartialReadStream(Stream baseStream, long basePosition, long length, bool ownStream = true) {
        if (!baseStream.CanRead) {
            throw new ArgumentException("Stream must be readable.", nameof(baseStream));
        }
        
        if (!baseStream.CanSeek) {
            throw new ArgumentException("Stream must be seekable.", nameof(baseStream));
        }
        
        _baseStream = baseStream;
        _ownStream = ownStream;
        _basePosition = basePosition;
        _length = length;

        if (_baseStream.Position < _basePosition) {
            _baseStream.Position = _basePosition;
        } else if (_baseStream.Position > _basePosition + _length) {
            _baseStream.Position = _basePosition + _length;
        }
    }

    public override void Flush() {
        throw new NotSupportedException();
    }

    public override void SetLength(long value) {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count) {
        throw new NotSupportedException();
    }

    public override void Write(ReadOnlySpan<byte> buffer) {
        throw new NotSupportedException();
    }

    public override void WriteByte(byte value) {
        throw new NotSupportedException();
    }
    
    public override long Seek(long offset, SeekOrigin origin) {
        if (!_baseStream.CanSeek) throw new NotSupportedException("Base stream does not support seeking.");

        switch (origin) {
            case SeekOrigin.Begin:
                if (offset < 0) {
                    throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset origin Begin must be greater than or equal to zero.");
                }

                offset = long.Min(offset, _length);
                _baseStream.Seek(_basePosition + offset, SeekOrigin.Begin);
                return offset;
            
            case SeekOrigin.Current:
                _baseStream.Seek(long.Clamp(_baseStream.Position + offset, _basePosition, _basePosition + _length), SeekOrigin.Begin);
                return _baseStream.Position - _basePosition;
            
            case SeekOrigin.End:
                if (offset > 0) {
                    throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset origin End must be less than or equal to zero.");
                }

                offset = long.Max(offset, -_length);
                _baseStream.Seek(_basePosition + offset, SeekOrigin.End);
                return _baseStream.Position - _basePosition;
            
            default: throw new ArgumentException("Invalid Seek Origin.");
        }
    }

    public override int Read(byte[] buffer, int offset, int count) {
        long p = _baseStream.Position;
        
        if (p + count > _basePosition + _length) {
            count = (int)(_basePosition + _length - p);
        }
        
        return _baseStream.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer) {
        long p = _baseStream.Position;
        
        if (p + buffer.Length > _basePosition + _length) {
            buffer = buffer[..(int)(_basePosition + _length - p)];
        }
        
        return _baseStream.Read(buffer);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (_ownStream) {
                _baseStream.Dispose();
                _baseStream = null!;
            }
        }
    }
}