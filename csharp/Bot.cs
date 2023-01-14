using Application.Models;
using Application.Models.Actions;
using Application.Models.Enemies;
using Application.Models.Towers;
using Application.Models.Shop;
using System.Linq;

namespace Application;

public class Bot
{
    public const string Name = "SMI";
    private GameMessage _gameMessage;
    private List<BaseAction> _actions;
    private List<String> _previousAttacks;
    public double currentMoney { get; set; } 
    private const string CANT_ATTACK = "CANT_ATTACK"; 
    // private int tickBeforeAttack;

    private Dictionary<Point, int> _gridBestSpikeTiles { get; set; }
    private Dictionary<Point, int> _gridBestSpearTiles { get; set; }
    
    public Bot()
    {
        Console.WriteLine("Initializing your super mega bot!");
        _actions = new List<BaseAction>();
        _gridBestSpikeTiles = new Dictionary<Point, int>();
        _gridBestSpearTiles = new Dictionary<Point, int>();
        _previousAttacks = new List<string>();
    }

    /// <summary>
    ///     Send back the list of actions to preform.
    /// </summary>
    public Task<List<BaseAction>> GetActionsAsync(GameMessage gameMessage, CancellationToken cancellationToken)
    {
        _gameMessage = gameMessage;
        currentMoney = _gameMessage.TeamInfos[_gameMessage.TeamId].Money;

        if(_gridBestSpikeTiles.Count == 0) {
            InitBestTiles();
        }
        
        _actions = new List<BaseAction>();
        
        int count = 0;
        while(BuyTower()) {
            count++;
            Console.WriteLine("Achat " + count);
            Console.WriteLine("Money" + currentMoney);
        }
        Attack();

        return Task.FromResult(_actions);
    }
    

    private int GetNeighboursCount(Point point, int dist) {
        var neighbours = new HashSet<Point>();
        neighbours.Add(new Point(point.X - dist, point.Y));
        neighbours.Add(new Point(point.X + dist, point.Y));
        neighbours.Add(new Point(point.X, point.Y - dist));
        neighbours.Add(new Point(point.X, point.Y + dist));
        neighbours.Add(new Point(point.X - dist, point.Y - dist));
        neighbours.Add(new Point(point.X + dist, point.Y + dist));
        neighbours.Add(new Point(point.X - dist, point.Y + dist));
        neighbours.Add(new Point(point.X + dist, point.Y - dist));

        int count = 0;
        foreach (var p in _gameMessage.Map.Paths) {
            foreach (var t in p.Tiles) {
                if (neighbours.Contains(t))
                    count++;
            }
        }

        return count;
    }
    
    private bool GridContains(int x, int y) {
        var grid = _gameMessage.PlayAreas[_gameMessage.TeamId].Grid;
        return !(!grid.ContainsKey(x) || !grid[x].ContainsKey(y));
    }

    private void InitBestTiles()
    {
        PlayArea PA = _gameMessage.PlayAreas[_gameMessage.TeamId];
        for (int y = 0; y < _gameMessage.Map.Height; y++) {
            for (int x = 0; x < _gameMessage.Map.Width; x++) {
                if(!GridContains(x, y)) {
                    Point p = new Point(x, y);
                    _gridBestSpikeTiles[p] = GetNeighboursCount(p, 1);
                    _gridBestSpearTiles[p] = GetNeighboursCount(p, 2);
                }       
            }
        }   
    }

    

    private Point GetMaxSpikeTile() 
    {
        if (_gridBestSpikeTiles.Count == 0) {
            var test = 10;
        }

        if (_gridBestSpikeTiles.Count != 0) {
            var max = _gridBestSpikeTiles.Max(x => x.Value);
            if(max >= 5)
                return _gridBestSpikeTiles.First(x => x.Value == max).Key;
        }

        return null;
    }

    private Point GetMaxSpearTile()
    {
        
        if (_gridBestSpearTiles.Count != 0)
        {
        var max = _gridBestSpearTiles.Max(x => x.Value);
           return _gridBestSpearTiles.First(x => x.Value == max).Key;
        }

        return null;
    }


    private List<TowerType> GetAvailableTowers() {
        var towers = _gameMessage.Shop.Towers;

        List<TowerType> affordableTowers = new List<TowerType>();
        foreach(var tower in towers) {
            if(tower.Value.Price <= currentMoney) { // TODO check if money left
                affordableTowers.Add(tower.Key);
            }
        }

        return affordableTowers;
    }


    private bool BuyTower() {
        bool hasBuilt = false;
        var affordableTowers = GetAvailableTowers();
        var p = GetMaxSpikeTile();

        if (affordableTowers.Contains(TowerType.SpikeShooter)) {
            if(p != null)
            {
                Console.WriteLine("SpikeShooter");
                _actions.Add(new Build(TowerType.SpikeShooter, p));
                currentMoney -= _gameMessage.Shop.Towers[TowerType.SpikeShooter].Price;
                _gridBestSpikeTiles.Remove(p);
                _gridBestSpearTiles.Remove(p);

                hasBuilt = true;
            }
        }

        if(!hasBuilt && p != null && _gridBestSpikeTiles[p] > 5)
        {
            return false;
        }

        var p2 = GetMaxSpearTile();
        if (p2 != null && !hasBuilt)
        {
            if (affordableTowers.Contains(TowerType.BombShooter) && !hasBuilt)
            {
                Console.WriteLine("BombShooter");
                _actions.Add(new Build(TowerType.BombShooter, p2));
                currentMoney -= _gameMessage.Shop.Towers[TowerType.BombShooter].Price;

                _gridBestSpikeTiles.Remove(p2);
                _gridBestSpearTiles.Remove(p2);

                hasBuilt = true;
            }

            if (affordableTowers.Contains(TowerType.SpearShooter) && !hasBuilt)
            {
                Console.WriteLine("SpearShooter");

                _actions.Add(new Build(TowerType.SpearShooter, p2));
                currentMoney -= _gameMessage.Shop.Towers[TowerType.SpearShooter].Price;

                _gridBestSpikeTiles.Remove(p2);
                _gridBestSpearTiles.Remove(p2);
                
                hasBuilt = true;
            }
        }

         return hasBuilt;
    }

    private EnemyType GetBestReinforcementAvailable() {
        var reinforcements = _gameMessage.Shop.Reinforcements;

        var affordableReinforcements = reinforcements.Where(i => i.Value.Price <= currentMoney).ToDictionary(i => i.Key, i => i.Value);
        
        double maxPrice = -1;
        EnemyType enemyToSend = EnemyType.Lvl1;

        foreach(var reinforcement in affordableReinforcements) {
            if(reinforcement.Value.Price > maxPrice) {
                enemyToSend = reinforcement.Key;
            }
        }
        return enemyToSend;
    }
    
    private string GetMaxHealthEnemy() {
        var enemies = _gameMessage.TeamInfos;
        
        var maxEnemies = enemies.Where(en => en.Value.IsAlive == true && en.Value.Id != _gameMessage.TeamId).OrderByDescending(team => team.Value.Hp).ToList();

        foreach(var enemy in maxEnemies) {
            if(CanAttack(enemy.Value.Id)) {
                return enemy.Value.Id;
            }
        }

        return CANT_ATTACK;
    }

    private void Attack() {
        if(_gameMessage.Shop.Reinforcements.Count == 0) return;
        
        var reinforcement = GetBestReinforcementAvailable();
        var enemyToAttack = GetMaxHealthEnemy();
        
        Console.WriteLine("Reinforcement sent: " + reinforcement);
        Console.WriteLine("Enemy to attack: " + enemyToAttack);

        if(enemyToAttack == CANT_ATTACK) return;

        _actions.Add(new SendReinforcements(reinforcement, enemyToAttack));
        _previousAttacks.Add(enemyToAttack);
        currentMoney -= _gameMessage.Shop.Reinforcements[reinforcement].Price;
    }

    // Check if previous attack has spawn
    private bool CanAttack(string teamId) => _previousAttacks.Count == 0 || HasReinforcementSpawn(teamId);

    private bool HasReinforcementSpawn(string teamId) {
        var ourTeamId = _gameMessage.TeamId;
        var enemyReinforcementQueue = _gameMessage.PlayAreas[teamId].enemyReinforcementsQueue;
        var reinforcementSent = enemyReinforcementQueue.Where(e => e.From == ourTeamId).ToList();

        if (reinforcementSent.Count != 0) {
            return false;
        }

        _previousAttacks.Remove(teamId);
        return true;
    }
    
}
