namespace Caxivitual.Lunacub.Tests.Building;

public class ProcessorOfTTests {
    [Fact]
    public void CanProcess_CorrectType_ReturnsTrue() {
        SimpleResourceProcessor processor = new();

        processor.CanProcess(new ResourceWithValueDTO()).Should().BeTrue();
    }

    [Fact]
    public void CanProcess_IncorrectType_ReturnsFalse() {
        SimpleResourceProcessor processor = new();

        processor.CanProcess(new ResourceWithReferenceDTO()).Should().BeFalse();
    }

    private sealed class SimpleResourceProcessor : Processor<ResourceWithValueDTO, ResourceWithValueDTO> {
        protected override ResourceWithValueDTO Process(ResourceWithValueDTO importedObject, ProcessingContext context) {
            return new();
        }
    }
}