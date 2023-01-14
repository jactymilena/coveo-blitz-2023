namespace Application.Models.Enemies;

public record Enemy(string Id,
                    EnemyType Type,
                    Point Position,
                    PrecisePoint PrecisePosition,
                    bool IsKilled,
                    bool HasEndedPath);
