using SimplePhotoImporter.Files.Utils;

namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  private static Dictionary<string, DateTimeOffset> GetAllFiles(string[] sourcePaths, string[] excludedSourcePaths, List<(DateTimeOffset Start, DateTimeOffset End)>? dateRanges, HashSet<string> photoExtensions, HashSet<string> videoExtensions, WayToGetShootingDateTime[] wayToGetShootingDateTime, int threadNumber)
  {
    // Modify photoExtensions to include photo and video extensions
    photoExtensions.UnionWith(videoExtensions);

    return sourcePaths
      .AsParallel()
      .AsOrdered()
      .WithDegreeOfParallelism(threadNumber)
      .Select(sourcePath => Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories))
      .SelectMany(x => x)
      .FilterAndToDictionary(excludedSourcePaths, dateRanges, photoExtensions, videoExtensions, wayToGetShootingDateTime);
  }

  private static Dictionary<string, DateTimeOffset> FilterAndToDictionary(this ParallelQuery<string> source, string[] excludedSourcePaths, List<(DateTimeOffset Start, DateTimeOffset End)>? dateRanges, HashSet<string> photoExtensions, HashSet<string> videoExtensions, WayToGetShootingDateTime[] wayToGetShootingDateTime)
  {
    if (dateRanges == null)
    {
      return source
        .Where(filePath => photoExtensions.Contains(Path.GetExtension(filePath).ToLower()) && !excludedSourcePaths.Any(excludedSourcePath => filePath.StartsWith(excludedSourcePath)))
        .ToDictionary(filePath => filePath, filePath => FileUtils.GetShootingDate(filePath, videoExtensions, wayToGetShootingDateTime));
    }
    else
    {
      return source
        .Where(filePath => photoExtensions.Contains(Path.GetExtension(filePath).ToLower()) && !excludedSourcePaths.Any(excludedSourcePath => filePath.StartsWith(excludedSourcePath)))
        .Select(filePath => (filePath, shootingDateTime: FileUtils.GetShootingDate(filePath, videoExtensions, wayToGetShootingDateTime)))
        .Where(fileInfo =>
        {
          foreach (var (Start, End) in dateRanges)
          {
            if (Start <= fileInfo.shootingDateTime && fileInfo.shootingDateTime <= End)
              return true;
          }
          return false;
        })
        .ToDictionary(fileInfo => fileInfo.filePath, fileInfo => fileInfo.shootingDateTime);
    }
  }
}
