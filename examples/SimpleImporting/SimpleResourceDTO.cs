﻿using System.Numerics;

namespace Caxivitual.Lunacub.Examples.SimpleResourceImporting;

public sealed class SimpleResourceDTO : ContentRepresentation {
    public int Integer;
    public float Single;
    public Vector2 Vector;

    public SimpleResourceDTO(int integer, float single, Vector2 vector) {
        Integer = integer;
        Single = single;
        Vector = vector;
    }
}