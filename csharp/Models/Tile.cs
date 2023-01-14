using Application.Models.Enemies;
using Application.Models.Towers;

namespace Application.Models;

public record Tile(string[] Paths, Enemy[] Enemies, Tower[] Towers, bool HasObstacle);
