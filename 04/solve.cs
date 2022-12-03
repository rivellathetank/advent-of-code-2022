Console.WriteLine(File.ReadLines("input").Select(FindItem).Sum(Priority));

char FindItem(string rucksack) {
  int n = rucksack.Length / 2;
  return rucksack[..n].Intersect(rucksack[n..]).First();
}

long Priority(char item) => item <= 'Z' ? item - 'A' + 27 : item - 'a' + 1;
