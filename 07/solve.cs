Console.WriteLine(
    File.ReadLines("input")
        .Select(s => s.Split(',').ToArray(r => r.Split('-').ToArray(int.Parse)))
        .Count(x => x[0][0] <= x[1][1] && x[1][0] <= x[0][1]));

static class Ext {
  public static U[] ToArray<T, U>(this IEnumerable<T> seq, Func<T, U> f) => seq.Select(f).ToArray();
}
