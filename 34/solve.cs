HashSet<int[]> surface = new(new Comparer());
foreach (int[] p in File.ReadLines("input").Select(Parse)) {
  for (int i = 0; i != p.Length; ++i) {
    for (int d = -1; d <= 1; d += 2) {
      int[] face = p.ToArray();
      face[i] += d;
      if (!surface.Add(face)) surface.Remove(face);
    }
  }
}
Console.WriteLine(surface.Count);

static int[] Parse(string s) =>
    s.Split(',').Select(int.Parse).Select(x => 2 * x).ToArray();

class Comparer : IEqualityComparer<int[]> {
  public bool Equals(int[] x, int[] y) => Enumerable.SequenceEqual(x, y);
  public int GetHashCode(int[] x) => x.Aggregate(0, HashCode.Combine);
}
