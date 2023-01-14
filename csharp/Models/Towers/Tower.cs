namespace Application.Models.Towers;

public record Tower(string Id, TowerType Type, Point Position, int Width, int Height, bool IsShooting);
