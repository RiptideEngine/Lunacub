namespace Caxivitual.Lunacub.Tests.Importing;

public class PartialReadStreamTests : IDisposable {
    private readonly MemoryStream _stream;

    public PartialReadStreamTests() {
        _stream = new(Enumerable.Range(0, 256).Select(Convert.ToByte).ToArray());
    }

    public void Dispose() {
        _stream.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Write_ShouldThrowNotSupportedException() {
        using PartialReadStream stream = new(_stream, 0, 256, false);

        new Action(() => stream.Write([], 0, 0)).Should().Throw<NotSupportedException>();
        new Action(() => stream.Write([])).Should().Throw<NotSupportedException>();
        new Action(() => stream.WriteByte(0)).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Flush_ShouldThrowNotSupportedException() {
        using PartialReadStream stream = new(_stream, 0, 256, false);
        
        new Action(() => stream.Flush()).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void SetLength_ShouldThrowNotSupportedException() {
        using PartialReadStream stream = new(_stream, 0, 256, false);
        
        new Action(() => stream.SetLength(0)).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ReadAll_ShouldBeCorrect() {
        using PartialReadStream stream = new(_stream, 0, 256, false);
        byte[] buffer = new byte[256];

        new Action(() => stream.ReadExactly(buffer)).Should().NotThrow();
        buffer.Should().Equal(Enumerable.Range(0, 256).Select(Convert.ToByte));
    }

    [Fact]
    public void ReadPartial_ShouldBeCorrect() {
        using PartialReadStream stream = new(_stream, 0, 256, false);
        byte[] buffer = new byte[128];
        
        new Action(() => stream.ReadExactly(buffer)).Should().NotThrow();
        buffer.Should().Equal(Enumerable.Range(0, 128).Select(Convert.ToByte));
    }
}