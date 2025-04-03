namespace Caxivitual.Lunacub.Tests.Building;

public class BuildingOptionsTests {
    [Fact]
    public void EquivalentEquality_ShouldBeCorrect() {
        BuildingOptions opt1 = new("A", "B");
        BuildingOptions opt2 = new("A", "B");

        opt1.Should().Be(opt2);
    }

    [Fact]
    public void DifferentEquality_ShouldBeCorrect() {
        BuildingOptions opt1 = new("A", "B");
        BuildingOptions opt2 = new("A", "C");
        
        opt1.Should().NotBe(opt2);
    }
}