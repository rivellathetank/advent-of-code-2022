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
