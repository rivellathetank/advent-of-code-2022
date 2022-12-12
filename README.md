# Advent of Code 2022

Solutions to [Advent of Code 2022](https://adventofcode.com/2022) puzzles.

- [Day 1](#day-1)
- [Day 2](#day-2)
- [Day 3](#day-3)
- [Day 4](#day-4)
- [Day 5](#day-5)
- [Day 6](#day-6)
- [Day 7](#day-7)
- [Day 8](#day-8)
- [Day 9](#day-9)
- [Day 10](#day-10)
- [Day 11](#day-11)

## Day 1

Each line in the first problem's input file is either a number or blank.
Consecutive numbers form groups. We need to find the maximum sum across all
groups.

I want to solve this with LINQ without sacrificing time or space complexity.
The solution would look like this:

```csharp
File.ReadLines("input")                 // read file lines
    .Split(s => s.Length == 0)          // split lines into groups
    .Select(seq => seq.Sum(int.Parse))  // convert groups to sums
    .Max();                             // get the max sum
```

The standard library doesn't have `Split()` that I'm using here, so I have to
implement it.

```csharp
// Splits a sequence based on the separator predicate. Omits empty subsequences.
//
// Time complexity: O(seq.Count()).
// Space complexity: O(1).
public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> seq, Func<T, bool> sep) {
  IEnumerator<T> e = seq.GetEnumerator();
  while (true) {
    do {
      if (!e.MoveNext()) yield break;
    } while (sep.Invoke(e.Current));
    yield return TakeUntil(e, sep);
  }

  static IEnumerable<T> TakeUntil(IEnumerator<T> e, Func<T, bool> sep) {
    do {
      yield return e.Current;
    } while (e.MoveNext() && !sep.Invoke(e.Current));
  }
}
```

I'm quite happy with this implementation. It invokes every method the minimum
possible number of times. My full solution is `O(N)` in time and `O(1)` in
space where `N` is the number of input lines. Cannot do any better.

For the second part of the day's puzzle we need to find the sum of three largest
groups. One way to do this with the tools I already have would be this:

```csharp
File.ReadLines("input")                 // read file lines
    .Split(s => s.Length == 0)          // split lines into groups
    .Select(seq => seq.Sum(int.Parse))  // convert groups to sums
    .OrderByDescending(x => x)          // sort from largest to smallest
    .Take(3)                            // get the first 3 elements
    .Sum();                             // get the sum
```

This would have `O(N + M * log(M))` time and `O(M)` space complexity where `N`
is the number of input lines and `M` is the number of groups. That's worse
than optimal on both counts, so I cannot afford it. I need to replace
`OrderByDescending().Take(3)` with the fused `TopN(3)`. There is no `TopN()` in
the standard library, so I'll implement it myself.

```csharp
// Returns Min(seq.Count(), n) largest elements in the sequence in ascending order.
//
// Requires: n >= 0.
// Time complexity: O(seq.Count() * Log(Min(seq.Count(), n))).
// Space complexity: Min(seq.Count(), n).
public static IEnumerable<T> TopN<T>(this IEnumerable<T> seq, int n) {
  if (n < 0) throw new ArgumentException("cannot be negative", nameof(n));
  List<T> top = new(2 * n);

  foreach (T x in seq) {
    top.Add(x);
    if (top.Count >= 2 * n) Shrink(top, n);
  }
  Shrink(top, n);
  return top;

  static void Shrink(List<T> top, int n) {
    top.Sort(static (x, y) => Comparer<T>.Default.Compare(y, x));
    if (top.Count > n) top.RemoveRange(n, top.Count - n);
  }
}
```

Another option would be to use a balanced tree with a capped size instead of
the list. Something like this:

```csharp
public static IEnumerable<T> TopN<T>(this IEnumerable<T> seq, int n) {
  if (n < 0) throw new ArgumentException("cannot be negative", nameof(n));
  SortedSet<(T Value, long _)> top = new();
  long idx = 0;
  foreach (T x in seq) {
    top.Add((x, ++idx));
    if (top.Count > n) top.Remove(top.First());
  }
  return top.Select(x => x.Value);
}
```

I would use this implementation if I needed to produce a rolling top N. I
haven't measured it but I think this implementation is slower for my use case
where I only need top N across all elements. In addition to the unavoidable
pointer chasing that's happening in the tree, there are a couple of
inefficiencies that aren't forced by the choice of the algorithm:

- The second field in the set's element type is a costly workaround for the lack
  of `SortedMultiSet` in the standard library.
- `top.Remove(top.First())` is an inefficient emulation of the missing
  `top.RemoveFirst()`.

Getting rid of these inefficiencies would require implementing a balanced tree
by hand.

My full solution to the second part of the puzzle is `O(N)` in time and `O(1)`
in space. Bliss.

## Day 2

Today we are playing Rock Paper Scissors with unorthodox scoring that depends
not only on whether you win/lose/draw but also on what you play. You get the
most points for winning with scissors and the least for losing with rock.
Unfortunately, we don't get to implement a winning strategy. Our job is to
simply calculate the score of matches that are given as input.

Input lines match `[A-C] [X-Z]`. The first letter encodes the first player's
move. The second letter is the second player's move in the first part of the
puzzle and the outcome of the match in the second part.

Here's my solution to the second part:

```csharp
File.ReadLines("input").Sum(p => Score(p[0] - 'A', p[2] - 'X'));
```

With this `Score()`:

```csharp
long Score(int a, int b) => (a + b + 2) % 3 + 1 + 3 * b;
```

`(a + b + 2) % 3` is the second player's move.

The solution to the first part is the same but with a different `Score()`.

## Day 3

The order of the day is intersection of sets. The number of sets to be
intersected is small and each of them is tiny, so any algorithm will do no
matter how ridiculously inefficient.

Here's part two:

```csharp
File.ReadLines("input")
    .Chunk(3)
    .Select(FindItem)
    .Sum(Priority)
```

With these helper functions:

```csharp
char FindItem(IEnumerable<string> group) =>
    group.Aggregate((a, b) => string.Concat(b.Intersect(a))).First();

long Priority(char item) =>
    item <= 'Z' ? item - 'A' + 27 : item - 'a' + 1;
```

Here `b.Intersect(a)` is more efficient than `a.Intersect(b)` because the
second argument gets converted to `HashSet`. This is undocumented but you can
see it in the [implementation](
  https://github.com/dotnet/runtime/blob/ebba1d4acb7abea5ba15e1f7f69d1d1311465d16/src/libraries/System.Linq/src/System/Linq/Intersect.cs#L78).

My code has optimal time complexity of `O(N)` where `N` is the number of input
characters. However, its space complexity is suboptimal as it's linear in group
size. Group size was fixed in the problem statement but I'm considering it a
runtime parameter for an additional challenge.

The optimal space complexity is `O(M)` where `M` is the maximum input string
length. It can be achieved by replacing the stock `Chunk()` from LINQ with this
version:

```csharp
// The same as Chunk() from LINQ but lazy. Space complexity is O(1) while Chunk()'s is O(n).
public static IEnumerable<IEnumerable<T>> LazyChunk<T>(this IEnumerable<T> seq, int n) {
  if (n <= 0) throw new ArgumentException("must be positive", nameof(n));
  IEnumerator<T> e = seq.GetEnumerator();
  while (e.MoveNext()) yield return Take(e, n);
  static IEnumerable<T> Take(IEnumerator<T> e, int n) {
    do {
      yield return e.Current;
    } while (--n != 0 && e.MoveNext());
  }
}
```

## Day 4

Each input line specifies a pair of non-trivial segments. For example,
`"2-8,3-7"` denotes `[2, 8]` and `[3, 7]`. We need to count pairs where one
segment contains (part 1) or overlaps (part 2) the other.

Part 1:

```csharp
File.ReadLines("input")
    .Select(s => s.Split(',').ToArray(r => r.Split('-').ToArray(int.Parse)))
    .Count(x => (x[0][0] - x[1][0]) * (x[0][1] - x[1][1]) <= 0);
```

Part 2:

```csharp
File.ReadLines("input")
    .Select(s => s.Split(',').ToArray(r => r.Split('-').ToArray(int.Parse)))
    .Count(x => x[0][0] <= x[1][1] && x[1][0] <= x[0][1]);
```

With this helper:

```csharp
public static U[] ToArray<T, U>(this IEnumerable<T> seq, Func<T, U> f) => seq.Select(f).ToArray();
```

LINQ has shortcuts for many common chains such as `.Where(f).Count()` or
`.Select(f).Sum()` but not for `.Select(f).ToArray()` or `.Select(f).ToList()`.
In my professional life I like to define overloads for all `.ToXXX()` methods to
make these constructs shorter. I've done the same in today's puzzle.

We are yet to see a problem that requires implementing some kind of algorithm.
Hopefully next week.

## Day 5

We are to implement a crane that moves crates according to specific
instructions: move `N` top crates from pile `A` to pile `B`, this sort of thing.
The initial placement of crates is also given. Just like before, no algorithms
required. A direct translation from *Human* to *Computer* is all we need to do.

Here's the second part:

```csharp
IEnumerator<string> input = File.ReadLines("input").GetEnumerator();
List<List<char>> stacks = new();

while (input.MoveNext()) {
  string line = input.Current;
  if (line[1] == '1') break;
  while (stacks.Count * 4 != line.Length + 1) stacks.Add(new());
  for (int i = 0; i != stacks.Count; ++i) {
    char crate = line[4 * i + 1];
    if (crate != ' ') stacks[i].Add(crate);
  }
}

input.MoveNext();
foreach (List<char> stack in stacks) stack.Reverse();

while (input.MoveNext()) {
  string[] parts = input.Current.Split(' ');
  int n = int.Parse(parts[1]);
  List<char> from = stacks[int.Parse(parts[3]) - 1];
  List<char> to = stacks[int.Parse(parts[5]) - 1];
  to.AddRange(from.Skip(from.Count - n));
  from.RemoveRange(from.Count - n, n);
}

Console.WriteLine(string.Join(null, stacks.Select(s => s.Last())));
```

## Day 6

We need to find the first sequence of `N` bytes in a file where all `N` are
distinct. `N` is 4 in the first and 14 in the second part of the puzzle.

Given the small size of the input file and small `N`, something as inefficient
as this would do the job:

```csharp
ReadOnlySpan<byte> input = File.ReadAllBytes("input");
for (int i = 0; ; ++i) {
  if (input[i..(i + N)].ToArray().Distinct().Count() == N) {
    Console.WriteLine(i + N);
    break;
  }
}
```

This is `O(M)` in size and `O(N * M)` in time where `M` is the size of input.

The best time complexity here is obviously `O(M)`, and the best space complexity
given given this time complexity is obviously `O(N)`. This is what I aimed to
implement.

```csharp
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
```

`FileStream` has a 4KB buffer, so reading one byte at a time isn't awful. Still,
a faster implementation would disable buffered reads, manually read up to 4KB at
a time, and iterate over that data. In addition, `input.Position >= N` doesn't
have to be in the main loop. Having a separate loop for initialization would get
rid of it.

## Day 7

We are given a shell typescript (as if recorded by script(1)) of a series of
`cd` and `ls` commands and are asked to compute disk usage by all directories.
For the first part of the puzzle we need to find all directories whose size is
at most 100000 and sum them up.

At first I solved this by constructing the complete filesystem in memory.
Something like this:

```csharp
class Dir {
  public Dir Parent { get; }
  public long Size { get; }
  public List<Dir> Children { get; }
}

Dir root;
```

Then I noticed that the input satisfies additional constraints that aren't
listed in the problem description:

- The first command in the listing is `cd /`.
- Each directory is entered only once.
- `ls` is executed in each directory only once.

All Advent of Code problems are underspecified and you always have to infer
missing constraints from the input file. Things like integer ranges, for
example, are always relevant and never specified. The formal rules of the game
only require that you submit *the answer*. Taking advantage of the particulars
of the input file is fair game. As a personal challenge I like to write code
that is a bit more generic than the minimum requirement but if extra unspecified
constraints allow me to write a more efficient algorithm, I'll do it.

In this case the fact that each directory and file is visited only once enables
the following algorithm:

```csharp
long ans = 0;
List<long> cwd = new() { 0, 0 };

foreach (string line in File.ReadLines("input").Skip(1)) {
  if (line == "$ cd ..") {
    Pop();
  } else if (line.StartsWith("$ cd ")) {
    cwd.Add(0);
  } else if (char.IsDigit(line[0])) {
    cwd[^1] += int.Parse(line[..line.IndexOf(' ')]);
  }
}

while (cwd.Count > 1) Pop();
Console.WriteLine(ans);

void Pop() {
  if (cwd[^1] <= 100000) ans += cwd[^1];
  cwd[^2] += cwd[^1];
  cwd.RemoveAt(cwd.Count - 1);
}
```

This is `O(N)` in time and `O(M)` in space where `N` is the size of the input
file and `M` is the maximum depth of the directory tree. I believe this is
optimal.

There is just one inefficiency that is bugging me: the first condition in the
loop implies the second, which means some duplicate work is being done. It
can be eliminated like this:

```csharp
if (line.StartsWith("$ cd ")) {
  if (line[5..] == "..") {
    ...
  } else {
    ...
  }
}
```

I didn't do this because it doesn't look as pretty.

## Day 8

We are given a square matrix of size `N` that represent tree heights in a
grid-shaped forest. For part 2 we need to count the number of trees that are
visible from the top of each tree when looking North, West, South and East,
multiply these numbers and find the maximum.

A trivial solution would be `O(N ^ 2)` in space and `O(N ^ 3)` in time, and
that's how I solved it initially. `N` is 99, so this solution is more than fast
enough.

I'm pretty sure `O(N ^ 2)` in space and time is optimal here. Here's my
implementation of it:

```csharp
string[] grid = File.ReadAllLines("input");
int n = grid.Length;

int[][] dist = {
  Dist((i, j) => (i, j)), Dist((i, j) => (i, n - j - 1)),
  Dist((i, j) => (j, i)), Dist((i, j) => (n - j - 1, i)),
};

Console.WriteLine(
    Enumerable.Range(0, n * n).Max(p => dist.Aggregate(1, (a, b) => a * b[p])));

int[] Dist(Func<int, int, (int, int)> pos) {
  int[] res = new int[n * n];
  Stack<(int Height, int Pos)> view = new();
  for (int i = 0; i != n; ++i) {
    view.Clear();
    view.Push((int.MaxValue, 0));
    for (int j = 0; j != n; ++j) {
      (int y, int x) = pos(i, j);
      while (view.Peek().Height < grid[y][x]) view.Pop();
      res[y * n + x] = j - view.Peek().Pos;
      view.Push((grid[y][x], j));
    }
  }
  return res;
}
```

## Day 9

Today we are simulating a rope on a plane. The movement of the rope's head on
each step is given in the input file: left, right up or down. The tail follows
with some degree of stretching.

Here's my solution for the second part where the rope has 10 connected nodes:

```csharp
HashSet<string> visited = new();
int[][] rope = Enumerable.Range(0, 10).Select(_ => new int[2]).ToArray();

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
```

I implemented it for N dimensions rather than just for 2 as was required. For
example, if the rope was allowed to move in 3 dimensions, I would need to
replace `2` with `3` at the top and add 5 new cases under `switch`.

This is the first solution of mine with superlinear time complexity in input
size. A linear solution is possible and not particularly difficult. I might get
back to it later.

*Edit*: I took shot at it and I have to retract what I wrote earlier about a
"not particularly difficult" linear solution. I can compute the position of
the rope in linear time but not the number of visited positions. I don't know
if a linear solution is possible for the latter. Anyway, here's my
half-solution that computes the movement of the rope in `O(N)` time and `O(1)`
space:

```csharp
int[][] rope = Enumerable.Range(0, 10).Select(_ => new int[2]).ToArray();

foreach (string line in File.ReadLines("input")) {
  int n = int.Parse(line[2..]);
  switch (line[0]) {
    case 'L': rope[0][0] -= n; break;
    case 'R': rope[0][0] += n; break;
    case 'U': rope[0][1] -= n; break;
    case 'D': rope[0][1] += n; break;
  }
  for (int i = 0; i != rope.Length - 1; ++i) {
    int[] d = rope[i].Zip(rope[i + 1]).Select(x => x.First - x.Second).ToArray();
    int m = d.Select(x => Math.Abs(x)).Max() - 1;
    if (m <= 0) continue;
    for (int j = 0; j != d.Length; ++j) {
      rope[i+1][j] += Math.Min(m, Math.Abs(d[j])) * Math.Sign(d[j]);
    }
  }
}
```

## Day 10

We are drawing pixels according to the instructions. The problem is trivial. I'm
starting to question why I'm even doing this.

```csharp
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
```

## Day 11

Today monkeys are throwing stolen items to each other in fancy ways. Part 1
doesn't require any thinking: you simply translate the problem statement into a
programming language of your choice and it spits out the answer. Part 2,
however, is the first real *puzzle* of this year's Advent of Code for it doesn't
provide the instructions for obtaining the answer. You have to come up with an
idea of your own. In particular, in order to solve part 2 the following
observations are necessary:

1. `(A + B) mod N = ((A mod N) + B) mod N`
2. `(A * B) mod N = ((A mod N) * B) mod N`

At the first glance I thought it would also require computing the lengths of
each item's cycle but unfortunately the number of rounds is too small for this
to be necessary. Here's my solution:

```csharp
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

long mod = monkeys.Select(m => m.Test).Aggregate((a, b) => a / Gcd(a, b) * b);

for (int i = 0; i != 10000; ++i) {
  foreach (Monkey m in monkeys) {
    foreach (long x in m.Items) {
      long y = m.Op(x) % mod;
      monkeys[y % m.Test == 0 ? m.Then : m.Else].Items.Add(y);
    }
    m.InspectCount += m.Items.Count;
    m.Items.Clear();
  }
}

monkeys.Sort((a, b) => b.InspectCount.CompareTo(a.InspectCount));
Console.WriteLine(monkeys[0].InspectCount * monkeys[1].InspectCount);

static long Gcd(long a, long b) {
  while (b != 0) (a, b) = (b, a % b);
  return a;
}

record Monkey(List<long> Items, Func<long, long> Op, long Test, int Then, int Else) {
  public long InspectCount { get; set; }
}
```

(Yes, that parsing code is obscene. Deal with it.)

This is linear in the number of rounds and independent of `mod`. Computing the
cycle lengths would flip these complexities: run time would be independent of
the number of rounds and linear in `mod`. In the actual puzzle `mod` was greater
than the number of rounds, so the simpler solution (listed above) is also more
efficient.

My solution computes `mod` as Least Common Multiple, although the numbers are
small enough that plain multiplication would've worked just as well.
