string[] grid = File.ReadAllLines("input");
int n = grid.Length;
bool[] vis = new bool[n * n];
Mark((i, j) => (i, j));
Mark((i, j) => (j, i));
Mark((i, j) => (i, n - j - 1));
Mark((i, j) => (n - j - 1, i));
Console.WriteLine(vis.Count(x => x));

void Mark(Func<int, int, (int, int)> pos) {
  for (int i = 0; i != n; ++i) {
    int max = -1;
    for (int j = 0; j != n; ++j) {
      (int y, int x) = pos(i, j);
      vis[y * n + x] |= grid[y][x] > max;
      max = Math.Max(grid[y][x], max);
    }
  }
}
