using System.Numerics;

namespace Caxivitual.Lunacub.Examples.ProceduralResources;

public record SimpleResource(int Value);

public sealed class SimpleResourceDTO : ContentRepresentation {
    public int Value;

    public SimpleResourceDTO(int value) {
        Value = value;
    }
}