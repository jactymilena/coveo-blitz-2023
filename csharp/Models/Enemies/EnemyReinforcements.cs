namespace Application.Models.Enemies;

public record EnemyReinforcements(EnemyType EnemyType,
                                  int Count,
                                  string From,
                                  string To);
