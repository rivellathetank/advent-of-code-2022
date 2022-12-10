int c = 0;
int x = 1;
int ans = 0;

foreach (string line in File.ReadLines("input")) {
  (int n, int a) = line == "noop" ? (1, 0) : (2, int.Parse(line[5..]));
  while (--n >= 0) {
    if (++c % 40 == 20) ans += c * x;
  }
  x += a;
}

Console.WriteLine(ans);
