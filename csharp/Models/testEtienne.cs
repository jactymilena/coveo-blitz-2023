// using Application.Models;
// using Application.Models.Actions;
// using Application.Models.Enemies;
// using Application.Models.Towers;
// using Application.Models.Shop;
// using System.Linq;

// namespace Application;

// public class TestEtienne
// {
//     public const string Name = "SMI";
//     private GameMessage _gameMessage;
//     private List<BaseAction> _actions;
//     private List<String> _previousAttacks;
//     public double currentMoney { get; set; } 
//     private const string CANT_ATTACK = "CANT_ATTACK"; 
//     private const int MAX_REINFORCEMENT = 8;
//     private List<Dictionary<Point, int>> _gridBestSpikeTiles { get; set; }
//     private List<Dictionary<Point, int>> _gridBestSpearTiles { get; set; }

//     public List<float> _pathsCoverage { get; set; }
//     public List<HashSet<Point>> _paths { get; set; }

//     private bool firsTurn;
//     public TestEtienne()
//     {
//         Console.WriteLine("Initializing your super mega bot!");
//         _actions = new List<BaseAction>();
//         _gridBestSpikeTiles = new List<Dictionary<Point, int>>();
//         _gridBestSpearTiles = new List<Dictionary<Point, int>>();
//         _previousAttacks = new List<string>();
//         _paths = new List<HashSet<Point>>();
//         _pathsCoverage = new List<float>();
//         firsTurn = true;
//     }

//     /// <summary>
//     ///     Send back the list of actions to preform.
//     /// </summary>
//     public Task<List<BaseAction>> GetActionsAsync(GameMessage gameMessage, CancellationToken cancellationToken)
//     {
//         _gameMessage = gameMessage;
//         currentMoney = _gameMessage.TeamInfos[_gameMessage.TeamId].Money;

//         if (firsTurn) {
//           InitBestTiles();
//           for(int i = 0; i < _gameMessage.Map.Paths.Count(); i++) {
//               _pathsCoverage.Add(0);
//              _paths.Add(new HashSet<Point>());
//               foreach(var t in _gameMessage.Map.Paths[i].Tiles)
//               {
//                 _paths[i].Add(t);
//               }

//           }
//             firsTurn = false;
//         }
        
//         _actions = new List<BaseAction>();
        
//         int count = 0;
//         while(BuyTower()) {
//             count++;
//             Console.WriteLine("Achat " + count);
//             Console.WriteLine("Money" + currentMoney);
//         }
//         Attack();

//         return Task.FromResult(_actions);
//     }
    

//     private void GetNeighboursCountForPath(Point point) {
//         var neighbours = new HashSet<Point>();
//         neighbours.Add(new Point(point.X - 1, point.Y));
//         neighbours.Add(new Point(point.X + 1, point.Y));
//         neighbours.Add(new Point(point.X, point.Y - 1));
//         neighbours.Add(new Point(point.X, point.Y + 1));
//         neighbours.Add(new Point(point.X - 1, point.Y - 1));
//         neighbours.Add(new Point(point.X + 1, point.Y + 1));
//         neighbours.Add(new Point(point.X - 1, point.Y + 1));
//         neighbours.Add(new Point(point.X + 1, point.Y - 1));

//         for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
//         {
//             int count = 0;
//             foreach (var t in _gameMessage.Map.Paths[i].Tiles)
//             {
//                 if (neighbours.Contains(t))
//                     count++;
//             }

//             this._gridBestSpikeTiles[i].Add(point, count);
//         }

//         neighbours.Add(new Point(point.X - 2, point.Y));
//         neighbours.Add(new Point(point.X + 2, point.Y));
//         neighbours.Add(new Point(point.X, point.Y - 2));
//         neighbours.Add(new Point(point.X, point.Y + 2));
//         neighbours.Add(new Point(point.X - 2, point.Y + 1));
//         neighbours.Add(new Point(point.X + 2, point.Y + 1));
//         neighbours.Add(new Point(point.X + 1, point.Y - 2));
//         neighbours.Add(new Point(point.X + 1, point.Y + 2));
//         neighbours.Add(new Point(point.X - 2, point.Y - 1));
//         neighbours.Add(new Point(point.X + 2, point.Y - 1));
//         neighbours.Add(new Point(point.X - 1, point.Y - 2));
//         neighbours.Add(new Point(point.X - 1, point.Y + 2));
//         neighbours.Add(new Point(point.X - 2, point.Y - 2));
//         neighbours.Add(new Point(point.X + 2, point.Y + 2));
//         neighbours.Add(new Point(point.X - 2, point.Y + 2));
//         neighbours.Add(new Point(point.X + 2, point.Y - 2));

//         for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
//         {
//             int count = 0;
//             foreach (var t in _gameMessage.Map.Paths[i].Tiles)
//             {
//                 if (neighbours.Contains(t))
//                     count++;
//             }

//             this._gridBestSpearTiles[i].Add(point, count);
//         }
//     }
    
//     private bool GridContains(int x, int y) {
//         var grid = _gameMessage.PlayAreas[_gameMessage.TeamId].Grid;
//         return !(!grid.ContainsKey(x) || !grid[x].ContainsKey(y));
//     }

//     private void InitBestTiles()
//     {
//       for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++) {
//         _gridBestSpikeTiles.Add(new Dictionary<Point, int>());
//         _gridBestSpearTiles.Add(new Dictionary<Point, int>());
//       }

//         PlayArea PA = _gameMessage.PlayAreas[_gameMessage.TeamId];
//         for (int y = 0; y < _gameMessage.Map.Height; y++) {
//             for (int x = 0; x < _gameMessage.Map.Width; x++) {
//                 if(!GridContains(x, y)) {
//                     Point p = new Point(x, y);
//                     GetNeighboursCountForPath(p);
//                 }       
//             }
//         }   
//     }

//     private int GetIdxWorstPathCoverage()
//     {
//       int pos = 0;
//       for (int i = 0; i < this._pathsCoverage.Count(); i++)
//       {
//           if (this._pathsCoverage[i] < this._pathsCoverage[pos]) { pos = i; }
//       }

//       return pos;
//     }

//     private Point GetMaxSpikeTile(int worstPathIndex) 
//     {
//         if (_gridBestSpikeTiles[worstPathIndex].Count != 0) {
//             var max = _gridBestSpikeTiles[worstPathIndex].Max(x => x.Value);
//             if(max >= 5)
//                 return _gridBestSpikeTiles[worstPathIndex].First(x => x.Value == max).Key;
//         }

//         return null;
//     }

//     private Point GetMaxSpearTile(int worstPathIndex)
//     {
        
//         if (_gridBestSpearTiles[worstPathIndex].Count != 0)
//         {
//           var max = _gridBestSpearTiles[worstPathIndex].Max(x => x.Value);
//            return _gridBestSpearTiles[worstPathIndex].First(x => x.Value == max).Key;
//         }

//         return null;
//     }


//     private List<TowerType> GetAvailableTowers() {
//         var towers = _gameMessage.Shop.Towers;

//         List<TowerType> affordableTowers = new List<TowerType>();
//         foreach(var tower in towers) {
//             if(tower.Value.Price <= currentMoney) { // TODO check if money left
//                 affordableTowers.Add(tower.Key);
//             }
//         }

//         return affordableTowers;
//     }

//     private void UpdateCoverage(Point origin, int range)
//     {
//         for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++){
//             for (int x = origin.X; x <= origin.X + range; x++) {
//                 for (int y = origin.Y; y <= origin.Y + range; y++){
                    

//                     // if (_gameMessage.Map.Paths[i])
//                     if (_paths[i].Contains(new Point(x, y)))
//                     {
//                         float newVal = _pathsCoverage[i] += (1.0f / (float)_gameMessage.Map.Paths.Count());
//                         _pathsCoverage[i] = newVal;
//                     }
//                 }
//         }
//       }
//     }

//     private bool BuyTower() {
//         bool hasBuilt = false;
//         var affordableTowers = GetAvailableTowers();
//         int worstPathCoverage = GetIdxWorstPathCoverage();
//         var p = GetMaxSpikeTile(worstPathCoverage);
    
//         if (affordableTowers.Contains(TowerType.SpikeShooter)) {
//             if(p != null)
//             {
//                 Console.WriteLine("SpikeShooter");
//                 _actions.Add(new Build(TowerType.SpikeShooter, p));
//                 currentMoney -= _gameMessage.Shop.Towers[TowerType.SpikeShooter].Price;

//                 for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
//                 {
//                   _gridBestSpearTiles[i].Remove(p);
//                   _gridBestSpikeTiles[i].Remove(p);
//                 }

//                 this.UpdateCoverage(p, 1);

//                 hasBuilt = true;
//             }
//         }

//         if(!hasBuilt && p != null && _gridBestSpikeTiles[worstPathCoverage][p] > 5)
//         {
//             return false;
//         }

//         var p2 = GetMaxSpearTile(worstPathCoverage);
//         if (p2 != null && !hasBuilt)
//         {
//             if (affordableTowers.Contains(TowerType.BombShooter) && !hasBuilt && _gameMessage.Round >= 5)
//             {
//                 Console.WriteLine("BombShooter");
//                 _actions.Add(new Build(TowerType.BombShooter, p2));
//                 currentMoney -= _gameMessage.Shop.Towers[TowerType.BombShooter].Price;

//                 for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
//                 {
//                   _gridBestSpearTiles[i].Remove(p2);
//                   _gridBestSpikeTiles[i].Remove(p2);
//                 }

//                 this.UpdateCoverage(p2, 2);

//                 hasBuilt = true;
//             }

//             if (affordableTowers.Contains(TowerType.SpearShooter) && !hasBuilt)
//             {
//                 Console.WriteLine("SpearShooter");

//                 _actions.Add(new Build(TowerType.SpearShooter, p2));
//                 currentMoney -= _gameMessage.Shop.Towers[TowerType.SpearShooter].Price;

//                 for (int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
//                 {
//                   _gridBestSpearTiles[i].Remove(p2);
//                   _gridBestSpikeTiles[i].Remove(p2);
//                 }

//                 this.UpdateCoverage(p2, 2);
                
//                 hasBuilt = true;
//             }
//         }

//          return hasBuilt;
//     }

//     private KeyValuePair<EnemyType, int> GetBestReinforcementAvailable() {
//         var reinforcements = _gameMessage.Shop.Reinforcements;

//         var affordableReinforcements = reinforcements.Where(i => i.Value.Price <= currentMoney).ToDictionary(i => i.Key, i => i.Value);
        
//         double maxPrice = -1;
//         EnemyType enemyToSend = EnemyType.Lvl0;
//         int enemyUnitCost = 1;

//         foreach(var reinforcement in affordableReinforcements) {
//             if(reinforcement.Value.Price > maxPrice) {
//                 enemyToSend = reinforcement.Key;
//                 enemyUnitCost = reinforcement.Value.Count;
//             }
//         }

//         int numberOfReinforcement = Math.Floor((float)(MAX_REINFORCEMENT / enemyUnitCost));
//         var numberOfReinforcement = Math.Floor((double)(MAX_REINFORCEMENT / enemyUnitCost));
//         return new KeyValuePair<EnemyType, int>(enemyToSend, numberOfReinforcement);
//     }
    
//     private string GetMaxHealthEnemy() {
//         var enemies = _gameMessage.TeamInfos;
        
//         var maxEnemies = enemies.Where(en => en.Value.IsAlive == true && en.Value.Id != _gameMessage.TeamId).OrderByDescending(team => team.Value.Hp).ToList();

//         foreach(var enemy in maxEnemies) {
//             if(CanAttack(enemy.Value.Id)) {
//                 return enemy.Value.Id;
//             }
//         }

//         return CANT_ATTACK;
//     }

//     private void Attack() {
//         if(_gameMessage.Shop.Reinforcements.Count == 0 || !AllPathCovered()) return;
        
//         var reinforcement = GetBestReinforcementAvailable();
//         var enemyToAttack = GetMaxHealthEnemy();
        
//         Console.WriteLine("Reinforcement sent: " + reinforcement);
//         Console.WriteLine("Enemy to attack: " + enemyToAttack);

//         if(enemyToAttack == CANT_ATTACK || reinforcement == EnemyType.Lvl0) return;

//         _actions.Add(new SendReinforcements(reinforcement, enemyToAttack));
//         _previousAttacks.Add(enemyToAttack);
//         currentMoney -= _gameMessage.Shop.Reinforcements[reinforcement].Price;
//     }

//     // Check if previous attack has spawn
//     private bool CanAttack(string teamId) => _previousAttacks.Count == 0 || HasReinforcementSpawn(teamId);

//     private bool HasReinforcementSpawn(string teamId) {
//         var ourTeamId = _gameMessage.TeamId;
//         var enemyReinforcementQueue = _gameMessage.PlayAreas[teamId].enemyReinforcementsQueue;
//         var reinforcementSent = enemyReinforcementQueue.Where(e => e.From == ourTeamId).ToList();

//         if (reinforcementSent.Count != 0) {
//             return false;
//         }

//         _previousAttacks.Remove(teamId);
//         return true;
//     }

//     private bool AllPathCovered() {
//         foreach(float f in _pathsCoverage) {
//             if(f == 0) {
//                 return false;
//             }
//         }
//         return true;
//     }
// }
