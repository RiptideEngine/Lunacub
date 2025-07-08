namespace Caxivitual.Lunacub.Tests.Importing;

public class StatisticsTests {
    private readonly Statistics _statistics = new();

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void AddReference_Parallel_HaveCorrectValues(uint times) {
        Parallel.For(0, times, _ => _statistics.AddReference());

        _statistics.TotalReferenceCount.Should().Be(times);
        _statistics.RemainReferenceCount.Should().Be(times);
        _statistics.UniqueResourceCount.Should().Be(0);
        _statistics.DisposedResourceCount.Should().Be(0);
        _statistics.UndisposedResourceCount.Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Release_Parallel_HaveCorrectValues(uint times) {
        _statistics.SetTotalReferenceCount(20000);
        _statistics.SetRemainReferenceCount(20000);

        Parallel.For(0, times, _ => _statistics.ReleaseReferences());
        
        _statistics.TotalReferenceCount.Should().Be(20000);
        _statistics.RemainReferenceCount.Should().Be(20000 - times);
        _statistics.UniqueResourceCount.Should().Be(0);
        _statistics.DisposedResourceCount.Should().Be(0);
        _statistics.UndisposedResourceCount.Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void IncrementUniqueResourceCount_Parallel_HaveCorrectValues(uint times) {
        Parallel.For(0, times, _ => _statistics.IncrementUniqueResourceCount());

        _statistics.TotalReferenceCount.Should().Be(0);
        _statistics.RemainReferenceCount.Should().Be(0);
        _statistics.UniqueResourceCount.Should().Be(times);
        _statistics.DisposedResourceCount.Should().Be(0);
        _statistics.UndisposedResourceCount.Should().Be(0);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void DecrementUniqueResourceCount_Parallel_HaveCorrectValues(uint times) {
        _statistics.SetUniqueResourceCount(20000);
        
        Parallel.For(0, times, _ => _statistics.DecrementUniqueResourceCount());

        _statistics.TotalReferenceCount.Should().Be(0);
        _statistics.RemainReferenceCount.Should().Be(0);
        _statistics.UniqueResourceCount.Should().Be(20000 - times);
        _statistics.DisposedResourceCount.Should().Be(0);
        _statistics.UndisposedResourceCount.Should().Be(0);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void IncrementDisposedResourceCount_Parallel_HaveCorrectValues(uint times) {
        Parallel.For(0, times, _ => _statistics.IncrementDisposedResourceCount());

        _statistics.TotalReferenceCount.Should().Be(0);
        _statistics.RemainReferenceCount.Should().Be(0);
        _statistics.UniqueResourceCount.Should().Be(0);
        _statistics.DisposedResourceCount.Should().Be(times);
        _statistics.UndisposedResourceCount.Should().Be(0);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void DecrementDisposedResourceCount_Parallel_HaveCorrectValues(uint times) {
        _statistics.SetDisposedResourceCount(20000);
        
        Parallel.For(0, times, _ => _statistics.DecrementDisposedResourceCount());

        _statistics.TotalReferenceCount.Should().Be(0);
        _statistics.RemainReferenceCount.Should().Be(0);
        _statistics.UniqueResourceCount.Should().Be(0);
        _statistics.DisposedResourceCount.Should().Be(20000 - times);
        _statistics.UndisposedResourceCount.Should().Be(0);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void IncrementUndisposedResourceCount_Parallel_HaveCorrectValues(uint times) {
        Parallel.For(0, times, _ => _statistics.IncrementUndisposedResourceCount());

        _statistics.TotalReferenceCount.Should().Be(0);
        _statistics.RemainReferenceCount.Should().Be(0);
        _statistics.UniqueResourceCount.Should().Be(0);
        _statistics.DisposedResourceCount.Should().Be(0);
        _statistics.UndisposedResourceCount.Should().Be(times);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void DecrementUndisposedResourceCount_Parallel_HaveCorrectValues(uint times) {
        _statistics.SetUndisposedResourceCount(20000);
        
        Parallel.For(0, times, _ => _statistics.DecrementUndisposedResourceCount());

        _statistics.TotalReferenceCount.Should().Be(0);
        _statistics.RemainReferenceCount.Should().Be(0);
        _statistics.UniqueResourceCount.Should().Be(0);
        _statistics.DisposedResourceCount.Should().Be(0);
        _statistics.UndisposedResourceCount.Should().Be(20000 - times);
    }
}