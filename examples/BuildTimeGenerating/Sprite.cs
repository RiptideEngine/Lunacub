namespace Caxivitual.Lunacub.Examples.BuildTimeGenerating;

public readonly record struct Rectangle(int X, int Y, uint Width, uint Height);
public readonly record struct Sprite(string Name, Rectangle Rect);