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
    private const int MAX_REINFORCEMENT = 8;
    private bool _attackFirst;

    private List<Dictionary<Point, int>> _gridBestSpikeTiles { get; set; }
    private List<Dictionary<Point, int>> _gridBestSpearTiles { get; set; }

    public List<float> _pathsCoverage { get; set; }
    public List<HashSet<Point>> _paths { get; set; }

    
    public bool _bomb { get; set; }
    public bool _bombBlocker { get; set; }
    public int _bombAmount { get; set; }

    private bool firsTurn;
    public Bot()
    {
        Console.WriteLine("Initializing your super mega bot!");
        _actions = new List<BaseAction>();
        _gridBestSpikeTiles = new List<Dictionary<Point, int>>();
        _gridBestSpearTiles = new List<Dictionary<Point, int>>();
        _previousAttacks = new List<string>();
        _paths = new List<HashSet<Point>>();
        _pathsCoverage = new List<float>();
        firsTurn = true;
        _bomb = false;
        _bombBlocker = true;
        _bombAmount = 0;
        _attackFirst = true;
    }

    /// <summary>
    ///     Send back the list of actions to preform.
    /// </summary>
    public Task<List<BaseAction>> GetActionsAsync(GameMessage gameMessage, CancellationToken cancellationToken)
    {
        _gameMessage = gameMessage;
        currentMoney = _gameMessage.TeamInfos[_gameMessage.TeamId].Money;

        // if (firsTurn) {
        //   InitBestTiles();
        //   for(int i = 0; i < _gameMessage.Map.Paths.Count(); i++) {
        //       _pathsCoverage.Add(0);
        //      _paths.Add(new HashSet<Point>());
        //       foreach(var t in _gameMessage.Map.Paths[i].Tiles)
        //       {
        //         _paths[i].Add(t);
        //       }

        //   }
        //     firsTurn = false;
        // }

        if (firsTurn)
        {
            InitBestTiles();
            for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
            {
                _pathsCoverage.Add(0);
                _paths.Add(new HashSet<Point>());
                foreach (var t in _gameMessage.Map.Paths[i].Tiles)
                {
                    _paths[i].Add(t);
                }

            }
            firsTurn = false;
        }

        if(!_bombBlocker && AllPath100Percent() && _bombAmount < _paths.Count()) 
        {
            _bomb = true;
        }
        else
        {
            _bomb = false;
        }//check for Bomb
        
        _actions = new List<BaseAction>();
        
        if(_attackFirst) Attack();

        int count = 0;
        while(BuyTower()) {
            count++;
            Console.WriteLine("Achat " + count);
            Console.WriteLine("Money" + currentMoney);
        }

        if(!_attackFirst) Attack();

        return Task.FromResult(_actions);
    }
    

    private void GetNeighboursCountForPath(Point point) {
        var neighbours = new HashSet<Point>();
        neighbours.Add(new Point(point.X - 1, point.Y));
        neighbours.Add(new Point(point.X + 1, point.Y));
        neighbours.Add(new Point(point.X, point.Y - 1));
        neighbours.Add(new Point(point.X, point.Y + 1));
        neighbours.Add(new Point(point.X - 1, point.Y - 1));
        neighbours.Add(new Point(point.X + 1, point.Y + 1));
        neighbours.Add(new Point(point.X - 1, point.Y + 1));
        neighbours.Add(new Point(point.X + 1, point.Y - 1));

        for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
        {
            int count = 0;
            foreach (var t in _gameMessage.Map.Paths[i].Tiles)
            {
                if (neighbours.Contains(t))
                    count++;
            }

            this._gridBestSpikeTiles[i].Add(point, count);
        }

        neighbours.Add(new Point(point.X - 2, point.Y));
        neighbours.Add(new Point(point.X + 2, point.Y));
        neighbours.Add(new Point(point.X, point.Y - 2));
        neighbours.Add(new Point(point.X, point.Y + 2));
        neighbours.Add(new Point(point.X - 2, point.Y + 1));
        neighbours.Add(new Point(point.X + 2, point.Y + 1));
        neighbours.Add(new Point(point.X + 1, point.Y - 2));
        neighbours.Add(new Point(point.X + 1, point.Y + 2));
        neighbours.Add(new Point(point.X - 2, point.Y - 1));
        neighbours.Add(new Point(point.X + 2, point.Y - 1));
        neighbours.Add(new Point(point.X - 1, point.Y - 2));
        neighbours.Add(new Point(point.X - 1, point.Y + 2));
        neighbours.Add(new Point(point.X - 2, point.Y - 2));
        neighbours.Add(new Point(point.X + 2, point.Y + 2));
        neighbours.Add(new Point(point.X - 2, point.Y + 2));
        neighbours.Add(new Point(point.X + 2, point.Y - 2));

        for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
        {
            int count = 0;
            foreach (var t in _gameMessage.Map.Paths[i].Tiles)
            {
                if (neighbours.Contains(t))
                    count++;
            }

            this._gridBestSpearTiles[i].Add(point, count);
        }
    }
    
    private bool GridContains(int x, int y) {
        var grid = _gameMessage.PlayAreas[_gameMessage.TeamId].Grid;
        return !(!grid.ContainsKey(x) || !grid[x].ContainsKey(y));
    }

    private void InitBestTiles()
    {
      for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++) {
        _gridBestSpikeTiles.Add(new Dictionary<Point, int>());
        _gridBestSpearTiles.Add(new Dictionary<Point, int>());
      }

        PlayArea PA = _gameMessage.PlayAreas[_gameMessage.TeamId];
        for (int y = 0; y < _gameMessage.Map.Height; y++) {
            for (int x = 0; x < _gameMessage.Map.Width; x++) {
                if(!GridContains(x, y)) {
                    Point p = new Point(x, y);
                    GetNeighboursCountForPath(p);
                }       
            }
        }   
    }

    private int GetIdxWorstPathCoverage()
    {
      int pos = 0;
      for (int i = 0; i < this._pathsCoverage.Count(); i++)
      {
          if (this._pathsCoverage[i] < this._pathsCoverage[pos]) { pos = i; }
      }

      return pos;
    }

    private Point GetMaxSpikeTile(int worstPathIndex) 
    {
        if (_gridBestSpikeTiles[worstPathIndex].Count != 0) {
            var max = _gridBestSpikeTiles[worstPathIndex].Max(x => x.Value);

              var possibilities = _gridBestSpikeTiles[worstPathIndex].Where(x => x.Value == max);
              int realMax = 0;
              Point realPos =  _gridBestSpikeTiles[worstPathIndex].First(x => x.Value == max).Key;

              foreach(var m in possibilities)
              {
                int maximum = 0;
                foreach (var path in this._gridBestSpikeTiles)
                {
                  maximum += path[m.Key];
                }

                if (maximum > realMax)
                {
                  realMax = maximum;
                  realPos = m.Key;
                }
              }

              if (realMax < 5)
                return null;

             return realPos;
        }

        return null;
    }
    private Point GetMaxSpearTile(int worstPathIndex)
    {
        if (_gridBestSpearTiles[worstPathIndex].Count != 0)
        {
            var max = _gridBestSpearTiles[worstPathIndex].Max(x => x.Value);

            var possibilities = _gridBestSpearTiles[worstPathIndex].Where(x => x.Value == max);
            int realMax = 0;
            Point realPos =  _gridBestSpearTiles[worstPathIndex].First(x => x.Value == max).Key;

            foreach(var m in possibilities)
            {
                int maximum = 0;
                foreach (var path in this._gridBestSpearTiles)
                {
                    maximum += path[m.Key];
                }

                if (maximum > realMax)
                {
                    realMax = maximum;
                    realPos = m.Key;
                }
            }

            if (realMax == 0)
                return null;
            return realPos;
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

    private void UpdateCoverage(Point origin, int range)
    {
        for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++){
            for (int x = origin.X - range; x <= origin.X + range; x++) {
                for (int y = origin.Y - range; y <= origin.Y + range; y++){
                    

                    // if (_gameMessage.Map.Paths[i])
                    if (_paths[i].Contains(new Point(x, y)))
                    {
                        float newVal = _pathsCoverage[i] + (1.0f / (float)_gameMessage.Map.Paths.Count());
                        _pathsCoverage[i] = newVal;
                    }
                }
        }
      }
    }

    private bool BuyTower()
    {
        bool hasBuilt = false;
        var affordableTowers = GetAvailableTowers();
        int worstPathCoverage = GetIdxWorstPathCoverage();
        var p = GetMaxSpikeTile(worstPathCoverage);

        if (affordableTowers.Contains(TowerType.SpikeShooter))
        {
            if (p != null)
            {
                Console.WriteLine("SpikeShooter");
                _actions.Add(new Build(TowerType.SpikeShooter, p));
                currentMoney -= _gameMessage.Shop.Towers[TowerType.SpikeShooter].Price;

                for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
                {
                    _gridBestSpearTiles[i].Remove(p);
                    _gridBestSpikeTiles[i].Remove(p);
                }

                this.UpdateCoverage(p, 1);

                hasBuilt = true;
            }
        }

        if (!hasBuilt && p != null && _gridBestSpikeTiles[worstPathCoverage][p] > 5)
        {
            return false;
        }
        Point BombPoint = null;
        if(_bomb)
        {
            var pathToBomb = _bombAmount;
            var start = this._gameMessage.Map.Paths[_bombAmount].Tiles[0];
            
            for (int x = start.X - 2; x <= start.X + 2; x++)
            {
                for (int y = start.Y - 2; y <= start.Y + 2; y++)
                {
                    Point TestBombPoint = new Point(x, y);
                    if (TileIsValid(x, y))
                    {
                        BombPoint = TestBombPoint;
                    }
                }
            }
            if (affordableTowers.Contains(TowerType.BombShooter) && BombPoint != null)
            {
                Console.WriteLine("BombShooter");
                _actions.Add(new Build(TowerType.BombShooter, BombPoint));
                currentMoney -= _gameMessage.Shop.Towers[TowerType.BombShooter].Price;

                for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
                {
                    _gridBestSpearTiles[i].Remove(BombPoint);
                    _gridBestSpikeTiles[i].Remove(BombPoint);
                }
                _bombAmount++;
                this.UpdateCoverage(BombPoint, 2);
                hasBuilt = true;
             }
        }

        if(BombPoint != null && !hasBuilt)
        {
            return false;
        }

        var p2 = GetMaxSpearTile(worstPathCoverage);
        if (p2 != null && !hasBuilt)
        {
            //if (affordableTowers.Contains(TowerType.BombShooter) && !hasBuilt && _gameMessage.Round >= 5)
            //{
            //    Console.WriteLine("BombShooter");
            //    _actions.Add(new Build(TowerType.BombShooter, p2));
            //    currentMoney -= _gameMessage.Shop.Towers[TowerType.BombShooter].Price;

            //    for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
            //    {
            //        _gridBestSpearTiles[i].Remove(p2);
            //        _gridBestSpikeTiles[i].Remove(p2);
            //    }

            //    this.UpdateCoverage(p2, 2);

            //    hasBuilt = true;
            //}

            if (affordableTowers.Contains(TowerType.SpearShooter) && !hasBuilt)
            {
                Console.WriteLine("SpearShooter");

                _actions.Add(new Build(TowerType.SpearShooter, p2));
                currentMoney -= _gameMessage.Shop.Towers[TowerType.SpearShooter].Price;

                for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
                {
                    _gridBestSpearTiles[i].Remove(p2);
                    _gridBestSpikeTiles[i].Remove(p2);
                }

                this.UpdateCoverage(p2, 2);

                hasBuilt = true;
            }
        }

        return hasBuilt;
    }

    private bool TileIsValid(int x, int y)
    {
      var grid = _gameMessage.PlayAreas[_gameMessage.TeamId].Grid;
      if (!grid.ContainsKey(x) || !grid[x].ContainsKey(y))
        return false;

      if (grid[x][y].HasObstacle || grid[x][y].Towers.Count() != 0 || grid[x][y].Paths.Count() != 0)
        return false;

       return true;
    }


    private KeyValuePair<EnemyType, int> GetBestReinforcementAvailable() {
        var reinforcements = _gameMessage.Shop.Reinforcements;

        var affordableReinforcements = reinforcements.Where(i => i.Value.Price <= currentMoney).ToDictionary(i => i.Key, i => i.Value);
        
        double maxPrice = -1;
        EnemyType enemyToSend = EnemyType.Lvl0;
        int maxNumberOfReinforcement = 1;

        foreach(var reinforcement in affordableReinforcements) {
        //     int numberOfReinforcement = (int)Math.Floor((float)(MAX_REINFORCEMENT / enemyUnitCost));
        //     Console.WriteLine("Number of reinforcement: " + maxNumberOfReinforcement);
        //     maxPrice = numberOfReinforcement * reinforcement.Value.Price;
            if(reinforcement.Value.Price >= maxPrice && currentMoney >= maxPrice) {
                // int numberOfReinforcement = (int)Math.Floor((float)(MAX_REINFORCEMENT / reinforcement.Value.Count));
                // while(currentMoney < numberOfReinforcement * reinforcement.Value.Price) {
                //     numberOfReinforcement -= reinforcement.Value.Count;
                // }
                int numberOfReinforcement = MAX_REINFORCEMENT;
                while(currentMoney < numberOfReinforcement * reinforcement.Value.Price) {
                    numberOfReinforcement -= 1;
                }
                enemyToSend = reinforcement.Key;
                maxPrice = reinforcement.Value.Price;
                maxNumberOfReinforcement = numberOfReinforcement;
            }
        }
        return new KeyValuePair<EnemyType, int>(enemyToSend, maxNumberOfReinforcement);
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
        if(_gameMessage.Shop.Reinforcements.Count == 0 || !AllPathCovered()) return;
        
        var vals = GetBestReinforcementAvailable();
        var reinforcement = vals.Key;
        var numberOfReinforcement = vals.Value;
        var enemyToAttack = GetMaxHealthEnemy();
        
        Console.WriteLine("Reinforcement sent: " + reinforcement);
        Console.WriteLine("Enemy to attack: " + enemyToAttack);

        if(enemyToAttack == CANT_ATTACK || reinforcement == EnemyType.Lvl0) return;

        for(var i = 0; i < numberOfReinforcement; i++) {
            _actions.Add(new SendReinforcements(reinforcement, enemyToAttack));
        }
        _previousAttacks.Add(enemyToAttack);
        currentMoney -= _gameMessage.Shop.Reinforcements[reinforcement].Price * numberOfReinforcement;
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
    
    private bool AllPathCovered() {
        foreach(float f in _pathsCoverage) {
            if(f == 0) {
                return false;
            }
        }
        return true;
    }

     private bool AllPath100Percent()
    {
        foreach (float f in _pathsCoverage)
        {
            Console.WriteLine("Coverage : " + f);
            if (f < 1.0f)
            {
                return false;
            }
        }
        return true;
    }


}
