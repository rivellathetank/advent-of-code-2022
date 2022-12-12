string[] grid = File.ReadAllLines("input");
HashSet<(int, int)> seen = new();
Queue<(int X, int Y, int H, int D)> queue = new();

int sy = 0, sx = grid.TakeWhile(s => (sy = s.IndexOf('S')) < 0).Count();
queue.Enqueue((sx, sy, 0, 0));
seen.Add((sx, sy));

do {
  (int px, int py, int ph, int pd) = queue.Dequeue();
  if (grid[px][py] == 'E') Console.WriteLine(pd);
  foreach ((int x, int y) in new [] {(px - 1, py), (px + 1, py), (px, py - 1), (px, py + 1)}) {
    if (x < 0 || x >= grid.Length || y < 0 || y >= grid[x].Length) continue;
    int h = (grid[x][y] == 'E' ? 'a' : grid[x][y]) - 'a';
    if (h <= ph + 1 && seen.Add((x, y))) queue.Enqueue((x, y, h, pd + 1));
  }
} while (queue.Count > 0);
