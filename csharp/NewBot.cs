using Application.Models;
using Application.Models.Actions;
using Application.Models.Enemies;
using Application.Models.Towers;
using Application.Models.Shop;
using System.Linq;

namespace Application;

public class NewBot
{
  public const string Name = "SMI";
  private GameMessage _gameMessage;
  public double currentMoney { get; set; } 

  public Spike spikeStrategy;
  public Spear spearStrategy;

  public bool firstTurn = true;

  public NewBot()
  {
    Console.WriteLine("Initializing your super mega bot!");
  }

  public Task<List<BaseAction>> GetActionsAsync(GameMessage gameMessage, CancellationToken cancellationToken)
  {
    this._gameMessage = gameMessage;

    if (this.firstTurn) {
      this.spikeStrategy = new Spike(gameMessage);
      this.spearStrategy = new Spear(gameMessage);

      this.firstTurn = false;
    }

    var actions = new List<BaseAction>();
    return Task.FromResult(actions);
  }
}