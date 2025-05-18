using Caxivitual.Lunacub.Importing.Extensions;

namespace Caxivitual.Lunacub.Tests.Importing;

public class StreamExtensionsTests {
    [Theory]
    [InlineData(0, 256)]
    [InlineData(32, 128)]
    [InlineData(128, 32)]
    [InlineData(128, 0)]
    public void CopyTo_InsideSourceRegion_CopyCorrectly(int offset, int count) {
        using MemoryStream source = new MemoryStream(Enumerable.Range(0, 256).Select(Convert.ToByte).ToArray());
        using MemoryStream destination = new MemoryStream();
        
        source.Seek(offset, SeekOrigin.Begin);
        
        StreamExtensions.CopyTo(source, destination, count).Should().Be(count);
        destination.ToArray().Should().Equal(Enumerable.Range(offset, count).Select(Convert.ToByte));
    }

    [Theory]
    [InlineData(128, 256)]
    [InlineData(256, 0)]
    [InlineData(256, 32)]
    public void CopyTo_SurpassedSourceEnd_StopCorrectly(int offset, int count) {
        using MemoryStream source = new MemoryStream(Enumerable.Range(0, 256).Select(Convert.ToByte).ToArray());
        using MemoryStream destination = new MemoryStream();
        
        source.Seek(offset, SeekOrigin.Begin);

        int expected = int.Min(count, 256 - offset);
        StreamExtensions.CopyTo(source, destination, count).Should().Be(expected);
        destination.ToArray().Should().Equal(Enumerable.Range(offset, expected).Select(Convert.ToByte));
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(-50)]
    [InlineData(-500)]
    public void CopyTo_NegativeCount_DoesNotThrowAndReturnsZero(int count) {
        using MemoryStream source = new MemoryStream(Enumerable.Range(0, 256).Select(Convert.ToByte).ToArray());
        using MemoryStream destination = new MemoryStream();
        
        StreamExtensions.CopyTo(source, destination, count).Should().Be(0);
        destination.Should().HaveLength(0);
    }
}