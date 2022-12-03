Console.WriteLine(
    File.ReadLines("input")
        .LazyChunk(3)
        .Select(FindItem)
        .Sum(Priority));

char FindItem(IEnumerable<string> group) =>
    group.Aggregate((a, b) => string.Concat(b.Intersect(a))).First();

long Priority(char item) => item <= 'Z' ? item - 'A' + 27 : item - 'a' + 1;

static class Ext {
  // The same as Chunk() from LINQ but lazy.
  public static IEnumerable<IEnumerable<T>> LazyChunk<T>(this IEnumerable<T> seq, int n) {
    if (n <= 0) throw new ArgumentException("must be positive", nameof(n));
    IEnumerator<T> e = seq.GetEnumerator();
    while (e.MoveNext()) yield return Take(e, n);
    static IEnumerable<T> Take(IEnumerator<T> e, int n) {
      do {
        yield return e.Current;
      } while (--n != 0 && e.MoveNext());
    }
  }
}
