namespace Application.Models;

public record Map(string Name, int Width, int Height, Path[] Paths, Point[] Obstacles);
