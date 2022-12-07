List<long> cwd = new() { 0, 0 };
List<long> dirs = new();

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
Console.WriteLine(dirs.Where(s => s >= cwd[0] - 40000000).Min());

void Pop() {
  dirs.Add(cwd[^1]);
  cwd[^2] += cwd[^1];
  cwd.RemoveAt(cwd.Count - 1);
}
