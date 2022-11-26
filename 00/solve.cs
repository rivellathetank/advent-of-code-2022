Console.WriteLine(
    File.ReadLines("input")
        .Split(s => s.Length == 0)
        .Select(seq => seq.Sum(int.Parse))
        .Max());

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
}
