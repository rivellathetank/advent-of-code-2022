using Jet = System.Func<ushort, ushort>;

Func<Jet> NextJet = Loop(
    File.ReadAllLines("input")[0]
        .Select<char, Jet>(c => c == '<' ? x => x <<= 1 : x => x >>= 1)
        .ToArray());

Func<ushort[]> NextShape = Loop(
    Shape(0b1111),
    Shape(0b0100,
          0b1110,
          0b0100),
    Shape(0b0010,
          0b0010,
          0b1110),
    Shape(0b1000,
          0b1000,
          0b1000,
          0b1000),
    Shape(0b1100,
          0b1100));

List<ushort> cave = new() { 0b111111111 };

for (int i = 0; i != 2022; ++i) {
  ushort[] shape = NextShape();
  for (int j = 0; j != 3 + shape.Length; ++j) cave.Add(0b100000001);
  for (int j = 0; ; ++j) {
    ushort[] shifted = shape.Select(NextJet()).ToArray();
    if (!Collide(shifted, cave, j)) shape = shifted;
    if (Collide(shape, cave, j + 1)) {
      for (int k = 1; k <= shape.Length; ++k) cave[^(j + k)] |= shape[^k];
      break;
    }
  }
  while (cave[^1] == 0b100000001) cave.RemoveAt(cave.Count - 1);
}

Console.WriteLine(cave.Count - 1);

static Func<T> Loop<T>(params T[] data) {
  int i = -1;
  return () => data[i = (i + 1) % data.Length];
}

static bool Collide(ushort[] s, List<ushort> c, int n) =>
    s.Zip(c.Skip(c.Count - s.Length - n)).Any(x => (x.First & x.Second) != 0);

ushort[] Shape(params ushort[] s) => s.Select(x => x <<= 2).Reverse().ToArray();
