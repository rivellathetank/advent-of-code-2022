string[] grid = File.ReadAllLines("input");
HashSet<(int, int)> seen = new();
Queue<(int X, int Y, int H, int D)> queue = new();

int sy = 0, sx = grid.TakeWhile(s => (sy = s.IndexOf('E')) < 0).Count();
queue.Enqueue((sx, sy, 0, 0));
seen.Add((sx, sy));

Loop:
(int px, int py, int ph, int pd) = queue.Dequeue();
foreach ((int x, int y) in new [] {(px - 1, py), (px + 1, py), (px, py - 1), (px, py + 1)}) {
  if (x < 0 || x >= grid.Length || y < 0 || y >= grid[x].Length) continue;
  int h = 'z' - (grid[x][y] == 'S' ? 'a' : grid[x][y]);
  if (h <= ph + 1 && seen.Add((x, y))) queue.Enqueue((x, y, h, pd + 1));
}
if (grid[px][py] != 'a') goto Loop;
Console.WriteLine(pd);
