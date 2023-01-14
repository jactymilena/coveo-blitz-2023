using Application.Models.Enemies;

namespace Application.Models.Actions;

public record SendReinforcements(EnemyType EnemyType,
                                 string Team) : BaseAction(ActionType.SendReinforcements);
