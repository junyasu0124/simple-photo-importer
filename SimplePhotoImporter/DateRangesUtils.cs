namespace SimplePhotoImporter;

public static class DateRangesUtils
{
  public static List<(DateTimeOffset Start, DateTimeOffset End)> Merge(List<(DateTimeOffset Start, DateTimeOffset End)> dateRanges)
  {
    dateRanges.Sort((a, b) => a.Start.CompareTo(b.Start));

    var result = new List<(DateTimeOffset Start, DateTimeOffset End)>();

    foreach (var current in dateRanges)
    {
      if (result.Count == 0)
      {
        result.Add(current);
        continue;
      }

      var (lastStart, lastEnd) = result[^1];

      if (current.Start <= lastEnd)
      {
        var merged = (lastStart, current.End > lastEnd ? current.End : lastEnd);
        result[^1] = merged;
      }
      else
      {
        result.Add(current);
      }
    }
    return result;
  }
}
