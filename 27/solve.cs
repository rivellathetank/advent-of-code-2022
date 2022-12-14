HashSet<(int X, int Y)> grid = new();
foreach (string[] p in File.ReadLines("input").Select(s => s.Split(" -> "))) {
  (int px, int py) = ParsePoint(p[0]);
  foreach ((int x, int y) in p.Select(ParsePoint)) {
    do {
      grid.Add((px += Math.Sign(x - px), py += Math.Sign(y - py)));
    } while (px != x || py != y);
  }
}

int floor = grid.Max(p => p.Y) + 2;

for (int ans = 1; ; ++ans) {
  (int x, int y) = (500, 0);
  while (++y != floor) {
    grid.Remove((x, y - 1));
    if (grid.Add((x, y)) || grid.Add((--x, y)) || grid.Add((x += 2, y))) continue;
    grid.Add((--x, --y));
    break;
  }
  if (y == 0) {
    Console.WriteLine(ans);
    break;
  }
}

static (int X, int Y) ParsePoint(string s) {
  int sep = s.IndexOf(',');
  return (int.Parse(s[..sep]), int.Parse(s[(sep + 1)..]));
}
