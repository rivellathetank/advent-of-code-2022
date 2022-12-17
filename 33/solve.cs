int jetIdx = -1;
Func<ushort, ushort>[] jets = File
    .ReadAllLines("input")[0]
    .Select<char, Func<ushort, ushort>>(c => c == '<' ? x => x <<= 1 : x => x >>= 1)
    .ToArray();

int shapeIdx = -1;
ushort[][] shapes = {
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
        0b1100),
};

int floor = 0;
List<ushort> cave = new() { 0b111111111 };
Dictionary<(int, int, int), (int Shape, int Floor)> seen = new();

for (int i = 1; ; ++i) {
  if (Drop(out int ds, out int df)) {
    long left = 1000000000000 - i;
    for (int j = 0; j != left % ds; ++j) Drop(out _, out _);
    Console.WriteLine(floor + cave.Count - 1 + left / ds * df);
    break;
  }
}

bool Drop(out int ds, out int df) {
  ushort[] shape = shapes[++shapeIdx % shapes.Length].ToArray();
  for (int i = 0; i != 3 + shape.Length; ++i) cave.Add(0b100000001);

  for (int i = 0; ; ++i) {
    Func<ushort, ushort> jet = jets[++jetIdx % jets.Length];
    ushort[] shifted = shape.Select(jet).ToArray();
    if (!Collide(shifted, cave, i)) shape = shifted;

    if (Collide(shape, cave, i + 1)) {
      for (int j = 1; j <= shape.Length; ++j) {
        if ((cave[^(i + j)] |= shape[^j]) == 0b111111111) {
          floor += cave.Count - i - j;
          cave.RemoveRange(0, cave.Count - i - j);
          var key = (
              shapeIdx % shapes.Length,
              jetIdx % jets.Length,
              cave.Aggregate(0, HashCode.Combine));
          if (seen.TryGetValue(key, out var x)) {
            ds = shapeIdx - x.Shape;
            df = floor - x.Floor;
            return true;
          } else {
            seen.Add(key, (shapeIdx, floor));
          }
          break;
        }
      }
      break;
    } else if (cave[^1] == 0b100000001) {
      --i;
      cave.RemoveAt(cave.Count - 1);
    }
  }

  ds = df = 0;
  return false;
}

static bool Collide(ushort[] s, List<ushort> c, int n) =>
    s.Zip(c.Skip(c.Count - s.Length - n)).Any(x => (x.First & x.Second) != 0);

ushort[] Shape(params ushort[] s) => s.Select(x => x <<= 2).Reverse().ToArray();
