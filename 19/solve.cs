int c = 0;
int x = 1;

foreach (string line in File.ReadLines("input")) {
  (int n, int a) = line == "noop" ? (1, 0) : (2, int.Parse(line[5..]));
  while (--n >= 0) {
    Console.Write(Math.Abs(c - x) <= 1 ? '#' : '.');
    c = (c + 1) % 40;
    if (c == 0) Console.WriteLine();
  }
  x += a;
}
