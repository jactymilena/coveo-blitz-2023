using Application.Models.Towers;

namespace Application.Models.Actions;

public record Build(TowerType TowerType,
                    Point Position) : BaseAction(ActionType.Build);
