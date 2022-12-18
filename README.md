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
- [Day 12](#day-12)
- [Day 13](#day-13)
- [Day 14](#day-14)
- [Day 15](#day-15)
- [Day 16](#day-16)
- [Day 17](#day-17)
- [Day 18](#day-18)

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

## Day 12

Today we are flooding a hiking trail. Both parts of the puzzle require thinking
up an algorithm. The problem description tells you the *properties* of the
solution but not the *steps* to obtain it.

I had fun code-golfing today. Here's my part 2:

```csharp
string[] grid = File.ReadAllLines("input");
HashSet<(int, int)> seen = new();
Queue<(int X, int Y, int H, int D)> queue = new();

int sy = 0, sx = grid.TakeWhile(s => (sy = s.IndexOf('E')) < 0).Count();
queue.Enqueue((sx, sy, 0, 0));
seen.Add((sx, sy));

Loop:
(int px, int py, int ph, int pd) = queue.Dequeue();
foreach ((int x, int y) in new [] {(px - 1, py), (px + 1, py), (px, py - 1), (px, py + 1)}) {
  if (x < 0 || x >= grid.Length || y < 0 || y >= grid[x].Length) continue;
  int h = 'z' - (grid[x][y] == 'S' ? 'a' : grid[x][y]);
  if (h <= ph + 1 && seen.Add((x, y))) queue.Enqueue((x, y, h, pd + 1));
}
if (grid[px][py] != 'a') goto Loop;
Console.WriteLine(pd);
```

The line with `TakeWhile()` finds the smallest pair `(sx, sy)` such that
`grid[sx][sy] == 'E'`. Isn't it marvelous? :D

Instead of `goto` I could've done something like this:

```csharp
for (int done = 0; done == 0;) {
  ...
  if (grid[px][py] == 'a') Console.WriteLine(done = pd);
}
```

I didn't do it because it adds an extra level of indentation and a condition
that gets evaluation on every iteration. Besides, I like `goto`.

## Day 13

Today we are processing recursive data structures. It's a step back in puzzle
difficulty. Unlike the previous two days, which required thinking, today we are
given a full description of the procedure we need to implement to obtain the
answer.

Here's my part 2:

```csharp
object[] d = { Parse("[[2]]"), Parse("[[6]]") };
List<object> packets =
    File.ReadLines("input")
        .Where(s => s.Length > 0)
        .Select(Parse)
        .Concat(d)
        .ToList();
packets.Sort(Cmp);
Console.WriteLine(d.Select(p => packets.IndexOf(p) + 1).Aggregate((a, b) => a * b));

static int Cmp(object a, object b) {
  List<object> x = a as List<object> ?? new() {(int)a};
  List<object> y = b as List<object> ?? new() {(int)b};
  for (int len = Math.Min(x.Count, y.Count), i = 0; i != len; ++i) {
    int cmp = a is int n && b is int m ? n.CompareTo(m) : Cmp(x[i], y[i]);
    if (cmp != 0) return cmp;
  }
  return x.Count.CompareTo(y.Count);
}

static object Parse(string s) {
  int i = 0;
  return Parse(s, ref i);

  static object Parse(string s, ref int i) {
    if (s[i] == '[') {
      List<object> res = new();
      for (++i; s[i] != ']';) {
        res.Add(Parse(s, ref i));
        if (s[i] == ',') ++i;
      }
      ++i;
      return res;
    } else {
      int res = 0;
      while (char.IsDigit(s[i])) res = 10 * res + (s[i++] - '0');
      return res;
    }
  }
}
```

## Day 14

Sifting sand through rocks. This problem is of the do-what-you-are-told variety.

My part 2:

```csharp
HashSet<(int X, int Y)> grid = new();
foreach (string[] p in File.ReadLines("input").Select(s => s.Split(" -> "))) {
  (int px, int py) = ParsePoint(p[0]);
  foreach ((int x, int y) in p.Select(ParsePoint)) {
    do {
      grid.Add((px += Math.Sign(x - px), py += Math.Sign(y - py)));
    } while (px != x || py != y);
  }
}

int floor = grid.Max(p => p.Y) + 2;

for (int ans = 1; ; ++ans) {
  (int x, int y) = (500, 0);
  while (++y != floor) {
    grid.Remove((x, y - 1));
    if (grid.Add((x, y)) || grid.Add((--x, y)) || grid.Add((x += 2, y))) continue;
    grid.Add((--x, --y));
    break;
  }
  if (y == 0) {
    Console.WriteLine(ans);
    break;
  }
}

static (int X, int Y) ParsePoint(string s) {
  int sep = s.IndexOf(',');
  return (int.Parse(s[..sep]), int.Parse(s[(sep + 1)..]));
}
```

## Day 15

For part 2 we need to find a point on a plain that lies in a given
4000000 by 4000000 square and do not lie in any of the given 25 Manhattan
circles. We are ensured that there is only one such point. Coordinates are
integers.

```csharp
const int N = 4000000;
List<(int X, int Y, int R)> sensors = new();
foreach (string[] w in File.ReadLines("input").Select(s => s.Split(' ').ToArray())) {
  int sx = int.Parse(w[2][2..^1]);
  int sy = int.Parse(w[3][2..^1]);
  int bx = int.Parse(w[8][2..^1]);
  int by = int.Parse(w[9][2..^0]);
  sensors.Add((sx, sy, Math.Abs(sx - bx) + Math.Abs(sy - by)));
}

for (int y = 0; y <= N; ++y) {
  List<(int L, int R)> segments = new() { (0, N) };
  foreach (var s in sensors) {
    int r = s.R - Math.Abs(s.Y - y);
    if (r < 0) continue;
    segments = segments.SelectMany(x => SegDiff(x, (s.X - r, s.X + r))).ToList();
  }
  if (segments.Count != 0) Console.WriteLine(segments[0].L * 4000000L + y);
}

static IEnumerable<(int, int)> SegDiff((int L, int R) a, (int L, int R) b) {
  int end = Math.Min(a.R, b.L - 1);
  if (end >= a.L) yield return (a.L, end);
  end = Math.Max(a.L, b.R + 1);
  if (end <= a.R) yield return (end, a.R);
}
```

My solution is `O(N * M)` where `N` is the length of the square and `M` is the
number of circles. Given that `N` is 4000000, it's quite slow. On my machine
it takes 9 seconds. With some micro-optimizations (iterate over indices instead
of using `foreach` and remove all allocations from the loop) I brought the run
time down to 260ms, so it's not terrible. It can also be trivially parallelized.
Still, I'm pretty sure there is a better algorithm -- perhaps `O(M *M)` -- that
I haven't thought of. I might get back to it later.

The solution for part 1 can be easily obtained by adapting this code. It would
have optimal time complexity: linear in input size. The solution I've committed
is slower. I'm keeping it because it looks nicer.

## Day 16

Today we are solving a variation of traveling salesman. The search space is
large enough that a straightforward search through it is going to be too slow.
One option is to micro-optimize it. Another and less principled option is to
add heuristics that make the algorithm inapplicable in general but still capable
of producing the right answer for the given input file. I've done a combination
of these.

```csharp
int n = 1;
Dictionary<string, (int Id, string[] Edges, int Rate)> input =
    File.ReadLines("input")
        .Select(s => s.Replace(",", "").Split(' ').ToArray())
        .ToDictionary(
            w => w[1],
            w => {
              int rate = int.Parse(w[4][5..^1]);
              return (w[1] == "AA" ? 0 : rate > 0 ? n++ : -1, w[9..], rate);
            });

int[] rate = new int[n];
int[][] dist = new int[n][];
foreach ((string name, var node) in input) {
  if (node.Id < 0) continue;
  rate[node.Id] = node.Rate;
  dist[node.Id] = new int[n];
  Queue<(string, int)> q = new();
  q.Enqueue((name, 0));
  HashSet<string> visited = new();
  while (q.Count > 0) {
    (string edge, int d) = q.Dequeue();
    if (!visited.Add(edge)) continue;
    var dst = input[edge];
    if (dst.Id >= 0) dist[node.Id][dst.Id] = d;
    foreach (string e in dst.Edges) q.Enqueue((e, d + 1));
  }
}

const int T = 26;
const int G = 20;
const int P = 5;

Console.WriteLine(Enumerable.Range(0, 1 << P).AsParallel().Max(Search));

int Search(int tid) => Enumerable
    .Range(0, (1 << (n - 1)))
    .Select(m => (uint)m)
    .Where(m => (m & ((1U << P) - 1)) == tid)
    .Max(m => Solve(m << 1) + Solve(~(m << 1)));

int Solve(uint mask) {
  List<State> states = new();
  List<Pending> pending = new() { new(0, new(0, 0, 0, mask | 1)) };

  for (int t = 0; ; ++t) {
    states.Clear();
    pending.Sort((x, y) => y.T.CompareTo(x.T));
    while (pending.Count > 0 && pending[^1].T == t) {
      states.Add(pending[^1].S);
      pending.RemoveAt(pending.Count - 1);
    }
    states.Sort((x, y) => y.Score.CompareTo(x.Score));
    if (t == T) return states[0].Score;
    foreach (State s in states.Take(G)) {
      pending.Add(new(t + 1, s with { Score = s.Score + s.Rate }));
      for (int j = 1; j != n; ++j) {
        uint v = s.Visited | (1U << j);
        if (v == s.Visited) continue;
        int dt = dist[s.Pos][j] + 1;
        pending.Add(new(t + dt, new(s.Score + dt * s.Rate, s.Rate + rate[j], j, v)));
      }
    }
  }
}

record State(int Score, int Rate, int Pos, uint Visited);
record struct Pending(int T, State S);
```

The first half isn't particularly interesting: just parsing and preprocessing.
The real algorithm starts after `G` is defined. This is a non-principled
that controls greediness of the search. If you make it smaller, the code runs
faster but may produce incorrect results.

## Day 17

Today we are simulating an infinite Tetris game. I like this one: you need to
come up with an algorithm but it's not too difficult.

Here's my part 1:

```csharp
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
```

It's inefficient but can be easily optimized.

## Day 18

Today we are measuring surface area in 3D. Fairly straightforward.

Part 1:

```csharp
HashSet<int[]> surface = new(new Comparer());
foreach (int[] p in File.ReadLines("input").Select(Parse)) {
  for (int i = 0; i != p.Length; ++i) {
    for (int d = -1; d <= 1; d += 2) {
      int[] face = p.ToArray();
      face[i] += d;
      if (!surface.Add(face)) surface.Remove(face);
    }
  }
}
Console.WriteLine(surface.Count);

static int[] Parse(string s) =>
    s.Split(',').Select(int.Parse).Select(x => 2 * x).ToArray();

class Comparer : IEqualityComparer<int[]> {
  public bool Equals(int[] x, int[] y) => Enumerable.SequenceEqual(x, y);
  public int GetHashCode(int[] x) => x.Aggregate(0, HashCode.Combine);
}
```

Part 2 is a bit code-golfy:

```csharp
HashSet<int[]> faces = new(new Comparer());
HashSet<int[]> solid = new(new Comparer());
foreach (int[] c in File.ReadLines("input").Select(Parse)) {
  for (int i = 0; i != 27; ++i) {
    solid.Add(new[] {c[0] + i % 3 - 1, c[1] + i / 3 % 3 - 1, c[2] + i / 9 % 3 - 1});
  }
  Adjacent(c).Count(p => faces.Add(p) || faces.Remove(p));
}

Dictionary<int[], bool> outside = new(new Comparer()) {
  [new[] {solid.Max(p => p[0]), 0, 0}] = true
};
Console.WriteLine(faces.Count(IsOutside));

bool IsOutside(int[] p) {
  bool res = false;
  Queue<int[]> q = new();
  HashSet<int[]> seen = new(new Comparer());
  for (int[] x = p; x != null; q.TryDequeue(out x)) {
    if (outside.TryGetValue(x, out res)) break;
    foreach (int[] a in Adjacent(x)) {
      if (!solid.Contains(a) && seen.Add(a)) q.Enqueue(a);
    }
  }
  foreach (int[] x in seen) outside[x] = res;
  return res;
}

static int[] Parse(string s) =>
    s.Split(',').Select(int.Parse).Select(x => 2 * x).ToArray();

static IEnumerable<int[]> Adjacent(int[] p) {
  for (int i = 0; i != 2 * p.Length; ++i) {
    int[] f = p.ToArray();
    f[i / 2] += i % 2 * 2 - 1;
    yield return f;
  }
}

class Comparer : IEqualityComparer<int[]> {
  public bool Equals(int[] x, int[] y) => Enumerable.SequenceEqual(x, y);
  public int GetHashCode(int[] x) => x.Aggregate(0, HashCode.Combine);
}
```
