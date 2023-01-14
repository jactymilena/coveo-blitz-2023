using Application.Models.Enemies;

namespace Application.Models;

public record Team(string Id, double Money, int Hp, bool IsAlive, double PayoutBonus, EnemyReinforcements[] sentReinforcements);
