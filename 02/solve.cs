Console.WriteLine(
    File.ReadLines("input")
        .Sum(p => Score(p[0] - 'A', p[2] - 'X')));

long Score(int a, int b) => b + 1 + (b - a + 4) % 3 * 3;
