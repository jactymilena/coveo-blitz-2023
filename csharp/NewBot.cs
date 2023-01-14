// using Application.Models;
// using Application.Models.Actions;
// using Application.Models.Enemies;
// using Application.Models.Towers;
// using Application.Models.Shop;
// using System.Linq;

// namespace Application;

// public class NewBot
// {
//     public const string Name = "SMI";
//     private GameMessage _gameMessage;
//     private List<BaseAction> _actions;
//     public double currentMoney { get; set; } 

//     private List<Dictionary<Point, int>> _gridBestSpikeTiles { get; set; }
//     private List<Dictionary<Point, int>> _gridBestSpearTiles { get; set; }

//     public List<float> _pathsCoverage { get; set; }
//     public List<HashSet<Point>> _paths { get; set; }

//     private bool firsTurn;
//     public NewBot()
//     {
//         Console.WriteLine("Initializing your super mega bot!");
//         _actions = new List<BaseAction>();
//         _gridBestSpikeTiles = new List<Dictionary<Point, int>>();
//         _gridBestSpearTiles = new List<Dictionary<Point, int>>();
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
//           for(int i = 0; i < _gameMessage.Map.Paths.Count(); i++)
//           {
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
//         var max = _gridBestSpikeTiles.Max(x => x.Value);

//         if (_gridBestSpikeTiles[worstPathIndex].Count == 0) {
//             var test = 10;
//         }

//         if (_gridBestSpikeTiles[worstPathIndex].Count == 0) {
//             var max = _gridBestSpikeTiles.Max(x => x.Value);
//             if(max >= 5)
//                 return _gridBestSpikeTiles[worstPathIndex].First(x => x.Value == max).Key;
//         }

//         return null;
//     }

//     private Point GetMaxSpearTile(int worstPathIndex)
//     {
        
//         if (_gridBestSpearTiles[worstPathIndex].Count != 0)
//         {
//         var max = _gridBestSpearTiles[worstPathIndex].Max(x => x.Value);
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

//         if(!hasBuilt && p != null && _gridBestSpikeTiles[p] > 5)
//         {
//             return false;
//         }

//         var p2 = GetMaxSpearTile(worstPathCoverage);
//         if (p2 != null && !hasBuilt)
//         {
//             if (affordableTowers.Contains(TowerType.BombShooter) && !hasBuilt)
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

//     private EnemyType GetBestReinforcementAvailable() {
//         var reinforcements = _gameMessage.Shop.Reinforcements;

//         var affordableReinforcements = reinforcements.Where(i => i.Value.Price <= currentMoney).ToDictionary(i => i.Key, i => i.Value);
        
//         double maxPrice = -1;
//         EnemyType enemyToSend = EnemyType.Lvl1;

//         foreach(var reinforcement in affordableReinforcements) {
//             if(reinforcement.Value.Price > maxPrice) {
//                 enemyToSend = reinforcement.Key;
//             }
//         }
//         return enemyToSend;
//     }
    
//     private string GetMaxHealthEnemy() {
//         var enemies = _gameMessage.TeamInfos;
        
//         var maxEnemy = enemies.Where(en => en.Value.IsAlive == true && en.Value.Id != _gameMessage.TeamId).MaxBy(k => k.Value.Hp);

//         return maxEnemy.Value.Id;
//     }

//     private void Attack() {
//         var reinforcement = GetBestReinforcementAvailable();
//         var enemyToAttack = GetMaxHealthEnemy();
        
//         Console.WriteLine("Reinforcement sent: " + reinforcement);
//         Console.WriteLine("Enemy to attack: " + enemyToAttack);

//         _actions.Add(new SendReinforcements(reinforcement, enemyToAttack));
//     }

    
// }
