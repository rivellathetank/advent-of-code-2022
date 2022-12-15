const int N = 4000000;
List<(int X, int Y, int R)> sensors = new();
foreach (string[] w in File.ReadLines("input").Select(s => s.Split(' ').ToArray())) {
  int sx = int.Parse(w[2][2..^1]);
  int sy = int.Parse(w[3][2..^1]);
  int bx = int.Parse(w[8][2..^1]);
  int by = int.Parse(w[9][2..^0]);
  sensors.Add((sx, sy, Math.Abs(sx - bx) + Math.Abs(sy - by)));
}

for (int y = 0; y <= N; ++y) {
  List<(int L, int R)> segments = new() { (0, N) };
  foreach (var s in sensors) {
    int r = s.R - Math.Abs(s.Y - y);
    if (r < 0) continue;
    segments = segments.SelectMany(x => SegDiff(x, (s.X - r, s.X + r))).ToList();
  }
  if (segments.Count != 0) Console.WriteLine(segments[0].L * 4000000L + y);
}

static IEnumerable<(int, int)> SegDiff((int L, int R) a, (int L, int R) b) {
  int end = Math.Min(a.R, b.L - 1);
  if (end >= a.L) yield return (a.L, end);
  end = Math.Max(a.L, b.R + 1);
  if (end <= a.R) yield return (end, a.R);
}
