// ReSharper disable AccessToDisposedClosure
namespace Caxivitual.Lunacub.Tests;

public class ChunkLookupTableTests {
    [Fact]
    public void Add_ShouldBeCorrect() {
        using ChunkLookupTable table = new();
        
        new Action(() => table.Add(0, 0)).Should().NotThrow();
        table.Count.Should().Be(1);
    }
    
    [Fact]
    public void AddResize_ShouldBeCorrect() {
        using ChunkLookupTable table = new(32);

        for (uint i = 0; i < table.Capacity; i++) {
            table.Add(i, (int)i);
        }
        
        table.Count.Should().Be(table.Capacity);
        
        int oldCapacity = table.Capacity;
        new Action(() => table.Add((uint)table.Capacity, 0)).Should().NotThrow();

        table.Count.Should().Be(oldCapacity + 1);
        table.Capacity.Should().BeGreaterThan(oldCapacity);

        (bool result, int position) = new Func<(bool, int)>(() => {
            bool result = table.TryGetChunkPosition(16, out int position);
            return (result, position);
        }).Should().NotThrow().Which;
        
        result.Should().BeTrue();
        position.Should().Be(16);
    }

    [Fact]
    public void TryGetChunkPosition_ShouldBeCorrect() {
        using ChunkLookupTable table = new(16);
        
        foreach (int value in Enumerable.Range(0, 16)) {
            table.Add((uint)value, value * 32);
        }
        
        new Func<(bool, int)>(() => {
            bool result = table.TryGetChunkPosition(15, out int position);
            return (result, position);
        }).Should().NotThrow().Which.Should().Be((true, 480));
        
        new Func<(bool, int)>(() => {
            bool result = table.TryGetChunkPosition(16, out int position);
            return (result, position);
        }).Should().NotThrow().Which.Should().Be((false, 0));
    }
}