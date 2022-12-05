# Advent of Code 2022

Solutions to [Advent of Code 2022](https://adventofcode.com/2022) puzzles.

- [Day 1](#day-1)
- [Day 2](#day-2)
- [Day 3](#day-3)
- [Day 4](#day-4)
- [Day 5](#day-5)

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
