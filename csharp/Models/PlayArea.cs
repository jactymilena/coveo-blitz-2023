using Application.Models.Enemies;
using Application.Models.Towers;

namespace Application.Models;

public record PlayArea(string TeamId,
                       Enemy[] Enemies,
                       EnemyReinforcements[] enemyReinforcementsQueue,
                       Tower[] Towers,
                       Dictionary<int, Dictionary<int, Tile>> Grid);
