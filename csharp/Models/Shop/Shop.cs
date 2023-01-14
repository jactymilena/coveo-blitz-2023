using Application.Models.Enemies;
using Application.Models.Towers;

namespace Application.Models.Shop;

public record Shop(Dictionary<TowerType, TowerEntry> Towers, Dictionary<EnemyType, ReinforcementsEntry> Reinforcements);
