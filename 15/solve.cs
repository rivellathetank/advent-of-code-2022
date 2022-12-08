string[] grid = File.ReadAllLines("input");
int n = grid.Length;

int[][] dist = {
  Dist((i, j) => (i, j)), Dist((i, j) => (i, n - j - 1)),
  Dist((i, j) => (j, i)), Dist((i, j) => (n - j - 1, i)),
};

Console.WriteLine(
    Enumerable.Range(0, n * n).Max(p => dist.Aggregate(1, (a, b) => a * b[p])));

int[] Dist(Func<int, int, (int, int)> pos) {
  int[] res = new int[n * n];
  Stack<(int Height, int Pos)> view = new();
  for (int i = 0; i != n; ++i) {
    view.Clear();
    view.Push((int.MaxValue, 0));
    for (int j = 0; j != n; ++j) {
      (int y, int x) = pos(i, j);
      while (view.Peek().Height < grid[y][x]) view.Pop();
      res[y * n + x] = j - view.Peek().Pos;
      view.Push((grid[y][x], j));
    }
  }
  return res;
}
