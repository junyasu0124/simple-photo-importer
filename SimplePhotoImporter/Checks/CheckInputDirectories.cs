namespace SimplePhotoImporter.Checks;

public static partial class Checks
{
  public static (string? Message, string[]? Paths, string[]? ExcludedPaths) CheckInputDirectories(string? input, bool isEnabledExcluded)
  {
    if (string.IsNullOrWhiteSpace(input))
    {
      return ("You must specify at least one path.", null, null);
    }
    else
    {
      var inputPaths = CustomRegexes.MatchInputPahts().Matches(input).Select(x => x.Value.ToLower().Trim('"').Trim('\\')).ToArray();

      if (inputPaths.Length == 0)
      {
        return ("You must specify at least one path.", null, null);
      }
      if ((isEnabledExcluded && inputPaths.Select(x => x.TrimStart('*')).Any(x => !Directory.Exists(x))) ||
         (!isEnabledExcluded && inputPaths.Any(x => !Directory.Exists(x))))
      {
        return ("The specified path does not exist.", null, null);
      }

      var paths = inputPaths.Where(x => !x.StartsWith('*')).ToArray();
      var excludedPaths = inputPaths.Where(x => x.StartsWith('*')).Select(x => x[1..]).ToArray();
      if (!isEnabledExcluded && excludedPaths.Length > 0)
      {
        return ("You cannot use the excluded path option.", null, null);
      }

      if (paths.Length == 0)
      {
        return ("You must specify at least one path.", null, null);
      }
      if (!excludedPaths.All(x => paths.Any(y => x.StartsWith(y + "\\"))))
      {
        return ("The excluded path must be a subdirectory of the specified path.", null, null);
      }
      if (paths.Any(x => paths.Any(y => x != y && y.StartsWith(x + "\\"))))
      {
        return ("The specified path must not be a subdirectory of another specified path.", null, null);
      }
      if (paths.Any(x => paths.Where(y => !ReferenceEquals(x, y)).Any(y => x == y || y.StartsWith(x + "\\"))) ||
        excludedPaths.Any(x => excludedPaths.Where(y => !ReferenceEquals(x, y)).Any(y => x == y || y.StartsWith(x + "\\"))))
      {
        return ("The specified path must not be a subdirectory of another specified path.", null, null);
      }
      return (null, paths, excludedPaths);
    }
  }
}
