HashSet<int[]> faces = new(new Comparer());
HashSet<int[]> solid = new(new Comparer());
foreach (int[] c in File.ReadLines("input").Select(Parse)) {
  for (int i = 0; i != 27; ++i) {
    solid.Add(new[] {c[0] + i % 3 - 1, c[1] + i / 3 % 3 - 1, c[2] + i / 9 % 3 - 1});
  }
  Adjacent(c).Count(p => faces.Add(p) || faces.Remove(p));
}

Dictionary<int[], bool> outside = new(new Comparer()) {
  [new[] {solid.Max(p => p[0]), 0, 0}] = true
};
Console.WriteLine(faces.Count(IsOutside));

bool IsOutside(int[] p) {
  bool res = false;
  Queue<int[]> q = new();
  HashSet<int[]> seen = new(new Comparer());
  for (int[] x = p; x != null; q.TryDequeue(out x)) {
    if (outside.TryGetValue(x, out res)) break;
    foreach (int[] a in Adjacent(x)) {
      if (!solid.Contains(a) && seen.Add(a)) q.Enqueue(a);
    }
  }
  foreach (int[] x in seen) outside[x] = res;
  return res;
}

static int[] Parse(string s) =>
    s.Split(',').Select(int.Parse).Select(x => 2 * x).ToArray();

static IEnumerable<int[]> Adjacent(int[] p) {
  for (int i = 0; i != 2 * p.Length; ++i) {
    int[] f = p.ToArray();
    f[i / 2] += i % 2 * 2 - 1;
    yield return f;
  }
}

class Comparer : IEqualityComparer<int[]> {
  public bool Equals(int[] x, int[] y) => Enumerable.SequenceEqual(x, y);
  public int GetHashCode(int[] x) => x.Aggregate(0, HashCode.Combine);
}
