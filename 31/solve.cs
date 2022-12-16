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
