object[] d = { Parse("[[2]]"), Parse("[[6]]") };
List<object> packets =
    File.ReadLines("input")
        .Where(s => s.Length > 0)
        .Select(Parse)
        .Concat(d)
        .ToList();
packets.Sort(Cmp);
Console.WriteLine(d.Select(p => packets.IndexOf(p) + 1).Aggregate((a, b) => a * b));

static int Cmp(object a, object b) {
  List<object> x = a as List<object> ?? new() {(int)a};
  List<object> y = b as List<object> ?? new() {(int)b};
  for (int len = Math.Min(x.Count, y.Count), i = 0; i != len; ++i) {
    int cmp = a is int n && b is int m ? n.CompareTo(m) : Cmp(x[i], y[i]);
    if (cmp != 0) return cmp;
  }
  return x.Count.CompareTo(y.Count);
}

static object Parse(string s) {
  int i = 0;
  return Parse(s, ref i);

  static object Parse(string s, ref int i) {
    if (s[i] == '[') {
      List<object> res = new();
      for (++i; s[i] != ']';) {
        res.Add(Parse(s, ref i));
        if (s[i] == ',') ++i;
      }
      ++i;
      return res;
    } else {
      int res = 0;
      while (char.IsDigit(s[i])) res = 10 * res + (s[i++] - '0');
      return res;
    }
  }
}
