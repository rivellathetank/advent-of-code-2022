List<Monkey> monkeys = File
    .ReadLines("input")
    .Chunk(7)
    .Select(lines => new Monkey(
      lines[1][18..].Split(", ").Select(long.Parse).ToList(),
      !long.TryParse(lines[2][25..], out long arg)
          ? x => x * x : lines[2][23] == '*' ? x => x * arg : x => x + arg,
      long.Parse(lines[3][21..]),
      int.Parse(lines[4][29..]),
      int.Parse(lines[5][30..])
    ))
    .ToList();

for (int i = 0; i != 20; ++i) {
  foreach (Monkey m in monkeys) {
    foreach (long x in m.Items) {
      long y = m.Op(x) / 3;
      monkeys[y % m.Test == 0 ? m.Then : m.Else].Items.Add(y);
    }
    m.InspectCount += m.Items.Count;
    m.Items.Clear();
  }
}

monkeys.Sort((a, b) => b.InspectCount.CompareTo(a.InspectCount));
Console.WriteLine(monkeys[0].InspectCount * monkeys[1].InspectCount);

record Monkey(List<long> Items, Func<long, long> Op, long Test, int Then, int Else) {
  public long InspectCount { get; set; }
}
