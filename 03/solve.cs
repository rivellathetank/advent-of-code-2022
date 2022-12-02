Console.WriteLine(
    File.ReadLines("input")
        .Sum(p => Score(p[0] - 'A', p[2] - 'X')));

long Score(int a, int b) => (a + b + 2) % 3 + 1 + 3 * b;
