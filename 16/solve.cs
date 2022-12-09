HashSet<string> visited = new();
int[][] rope = Enumerable.Range(0, 2).Select(_ => new int[2]).ToArray();

foreach (string line in File.ReadLines("input")) {
  for (int i = int.Parse(line[2..]); i != 0; --i) {
    switch (line[0]) {
      case 'L': --rope[0][0]; break;
      case 'R': ++rope[0][0]; break;
      case 'U': --rope[0][1]; break;
      case 'D': ++rope[0][1]; break;
    }
    for (int j = 0; j != rope.Length - 1; ++j) {
      if (rope[j].Zip(rope[j + 1]).Any(x => Math.Abs(x.First - x.Second) > 1)) {
        for (int k = 0; k != rope[0].Length; ++k) {
          rope[j+1][k] += Math.Sign(rope[j][k] - rope[j+1][k]);
        }
      }
    }
    visited.Add(string.Join(' ', rope[^1]));
  }
}

Console.WriteLine(visited.Count);
