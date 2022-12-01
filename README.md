# Advent of Code 2022

Solutions to [Advent of Code 2022](https://adventofcode.com/2022) puzzles.

- [Day 1](#day-1)

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
