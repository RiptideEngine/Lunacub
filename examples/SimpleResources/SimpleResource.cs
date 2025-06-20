﻿namespace Caxivitual.Lunacub.Examples.SimpleResources;

public record SimpleResource(int Value);

public sealed class SimpleResourceDTO : ContentRepresentation {
    public int Value;

    public SimpleResourceDTO(int value) {
        Value = value;
    }
}