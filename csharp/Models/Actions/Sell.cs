namespace Application.Models.Actions;

public record Sell(Point Position) : BaseAction(ActionType.Sell);
