Console.WriteLine(
    File.ReadLines("input")
        .Split(s => s.Length == 0)
        .Select(seq => seq.Sum(int.Parse))
        .TopN(3)
        .Sum());

static class Ext {
  // Splits a sequence based on the separator predicate. Omits empty subsequences.
  //
  // Time complexity: O(seq.Count()).
  // Space complexity: O(1).
  public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> seq, Func<T, bool> sep) {
    IEnumerator<T> e = seq.GetEnumerator();
    while (true) {
      do {
        if (!e.MoveNext()) yield break;
      } while (sep.Invoke(e.Current));
      yield return TakeUntil(e, sep);
    }

    static IEnumerable<T> TakeUntil(IEnumerator<T> e, Func<T, bool> sep) {
      do {
        yield return e.Current;
      } while (e.MoveNext() && !sep.Invoke(e.Current));
    }
  }

  // Returns Min(seq.Count(), n) largest elements in the sequence in ascending order.
  //
  // Requires: n >= 0.
  // Time complexity: O(seq.Count() * Log(Min(seq.Count(), n))).
  // Space complexity: Min(seq.Count(), n).
  public static IEnumerable<T> TopN<T>(this IEnumerable<T> seq, int n) {
    if (n < 0) throw new ArgumentException("cannot be negative", nameof(n));
    List<T> top = new(2 * n);

    foreach (T x in seq) {
      top.Add(x);
      if (top.Count >= 2 * n) Shrink(top, n);
    }
    Shrink(top, n);
    return top;

    static void Shrink(List<T> top, int n) {
      top.Sort(static (x, y) => Comparer<T>.Default.Compare(y, x));
      if (top.Count > n) top.RemoveRange(n, top.Count - n);
    }
  }
}
