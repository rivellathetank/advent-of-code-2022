const int N = 4;

byte[] window = new byte[N];
byte[] hist = new byte[256];
Span<byte> buf = stackalloc byte[1];
using FileStream input = File.Open("input", FileMode.Open);

for (int n = 0, p = 0; n != N; p = (p + 1) % N) {
  if (input.Read(buf) != 1) throw new Exception("unexpected EOF");
  if (input.Position >= N && --hist[window[p]] == 0) --n;
  if (++hist[buf[0]] == 1) ++n;
  window[p] = buf[0];
}

Console.WriteLine(input.Position);
