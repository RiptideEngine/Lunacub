namespace Caxivitual.Lunacub.Examples.BuildTimeGeneration;

public readonly record struct Rectangle(int X, int Y, uint Width, uint Height);
public readonly record struct Sprite(string Name, Rectangle Rect);