const int Y = 2000000;
HashSet<int> beacons = new();
HashSet<int> covered = new();
foreach (string[] w in File.ReadLines("input").Select(s => s.Split(' ').ToArray())) {
  int sx = int.Parse(w[2][2..^1]);
  int sy = int.Parse(w[3][2..^1]);
  int bx = int.Parse(w[8][2..^1]);
  int by = int.Parse(w[9][2..^0]);
  int r = Math.Abs(sx - bx) + Math.Abs(sy - by);
  if (by == Y) beacons.Add(bx);
  for (int i = r - Math.Abs(sy - Y); i >= 0; --i) {
    covered.Add(sx + i);
    covered.Add(sx - i);
  }
}
Console.WriteLine(covered.Except(beacons).Count());
