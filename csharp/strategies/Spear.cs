using Application.Models;
namespace Application;

public class Spear 
{
    private GameMessage _gameMessage;
  private Dictionary<Point, int> _gridBestTiles { get; set; }

  public Spear(GameMessage gm) 
  {
    this._gridBestTiles = new Dictionary<Point, int>();
    this._gameMessage = gm;
  }

  public Decision BestTile() 
  {
    var max = this._gridBestTiles.Max(x => x.Value);
    var p = this._gridBestTiles.First(x => x.Value == max).Key;

    Decision d;
    d.p = p;
    d.Weight = this._gridBestTiles[p];
    return d;
  }

  public Decision BestTile(int pathIdx) {
    List<Models.Path> paths = new List<Models.Path>();
    paths.Add(this._gameMessage.Map.Paths[pathIdx]);

    var tiles = this.CalcBestTiles(paths);
    var max = tiles.Max(x => x.Value);
    var p = tiles.First(x => x.Value == max).Key;

    Decision d;
    d.p = p;
    d.Weight = tiles[p];
    return d;
  }

  public void Take(Point p) 
  {
    this._gridBestTiles.Remove(p);
  }

  private int GetNeighboursCount(Point p, List<Models.Path> paths) 
  {
    var neighbours = new HashSet<Point>();
    neighbours.Add(new Point(p.X - 1, p.Y));
    neighbours.Add(new Point(p.X + 1, p.Y));
    neighbours.Add(new Point(p.X, p.Y - 1));
    neighbours.Add(new Point(p.X, p.Y + 1));
    neighbours.Add(new Point(p.X - 1, p.Y - 1));
    neighbours.Add(new Point(p.X + 1, p.Y + 1));
    neighbours.Add(new Point(p.X - 1, p.Y + 1));
    neighbours.Add(new Point(p.X + 1, p.Y - 1));
    neighbours.Add(new Point(p.X - 2, p.Y));
    neighbours.Add(new Point(p.X + 2, p.Y));
    neighbours.Add(new Point(p.X, p.Y - 2));
    neighbours.Add(new Point(p.X, p.Y + 2));
    neighbours.Add(new Point(p.X - 2, p.Y - 2));
    neighbours.Add(new Point(p.X + 2, p.Y + 2));
    neighbours.Add(new Point(p.X - 2, p.Y + 2));
    neighbours.Add(new Point(p.X + 2, p.Y - 2));

    int count = 0;
    foreach (var point in paths) 
    {
        foreach (var t in point.Tiles) 
        {
            if (neighbours.Contains(t))
                count++;
        }
    }

    return count;
  }

  private Dictionary<Point, int> CalcBestTiles(List<Models.Path> paths) {
    PlayArea pa = this._gameMessage.PlayAreas[this._gameMessage.TeamId];
    var grid = this._gameMessage.PlayAreas[this._gameMessage.TeamId].Grid;

    Dictionary<Point, int> d = new Dictionary<Point, int>();

    for (int y = 0; y < _gameMessage.Map.Height; y++) {
        for (int x = 0; x < _gameMessage.Map.Width; x++) {    
            if (grid.ContainsKey(x) && grid[x].ContainsKey(y)) {
              Point p = new Point(x, y);

              d.Add(p, GetNeighboursCount(p, paths));
            }
        }
      }

    return d;
  }
}