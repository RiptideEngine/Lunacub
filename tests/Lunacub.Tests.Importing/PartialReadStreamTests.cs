// ReSharper disable AccessToDisposedClosure

namespace Caxivitual.Lunacub.Tests.Importing;

public class PartialReadStreamTests : IDisposable {
    private readonly MemoryStream _stream = new(Enumerable.Range(0, 256).Select(Convert.ToByte).ToArray());

    public void Dispose() {
        _stream.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void WriteOverloads_ThrowsNotSupportedException() {
        using PartialReadStream stream = new(_stream, 0, 256, false);

        new Action(() => stream.Write([], 0, 0)).Should().Throw<NotSupportedException>();
        new Action(() => stream.Write([])).Should().Throw<NotSupportedException>();
        new Action(() => stream.WriteByte(0)).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Flush_ThrowsNotSupportedException() {
        using PartialReadStream stream = new(_stream, 0, 256, false);
        
        new Action(() => stream.Flush()).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void SetLength_ThrowsNotSupportedException() {
        using PartialReadStream stream = new(_stream, 0, 256, false);
        
        new Action(() => stream.SetLength(0)).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Read_Everything_ShouldBeCorrect() {
        using PartialReadStream stream = new(_stream, 0, 256, false);
        byte[] buffer = new byte[256];

        new Action(() => stream.ReadExactly(buffer)).Should().NotThrow();
        buffer.Should().Equal(Enumerable.Range(0, 256).Select(Convert.ToByte));
    }

    [Fact]
    public void Read_Partial_ShouldBeCorrect() {
        using PartialReadStream stream = new(_stream, 0, 256, false);
        byte[] buffer = new byte[128];
        
        new Action(() => stream.ReadExactly(buffer)).Should().NotThrow();
        buffer.Should().Equal(Enumerable.Range(0, 128).Select(Convert.ToByte));
    }
}