namespace Caxivitual.Lunacub.Tests.Building;

public class ProcessorOfTTests {
    [Fact]
    public void CanProcess_CorrectType_ReturnsTrue() {
        SimpleResourceProcessor processor = new();

        processor.CanProcess(new SimpleResourceDTO()).Should().BeTrue();
    }

    [Fact]
    public void CanProcess_IncorrectType_ReturnsFalse() {
        SimpleResourceProcessor processor = new();

        processor.CanProcess(new ReferencingResourceDTO()).Should().BeFalse();
    }

    private sealed class SimpleResourceProcessor : Processor<SimpleResourceDTO, SimpleResourceDTO> {
        protected override SimpleResourceDTO Process(SimpleResourceDTO importedObject, ProcessingContext context) {
            return new();
        }
    }
}