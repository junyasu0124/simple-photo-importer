using SimplePhotoImporter.Files.Utils;

namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  private static Dictionary<string, DateTimeOffset> GetAllFiles(string[] sourcePaths, string[] excludedSourcePaths, string[] photoExtensions, string[] videoExtensions, WayToGetShootingDateTime[] wayToGetShootingDateTime)
  {
    string[] extensions = [.. photoExtensions, .. videoExtensions];

    return sourcePaths
      .Select(sourcePath => Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories))
      .SelectMany(x => x)
      .Where(file => extensions.Contains(Path.GetExtension(file).ToLower()))
      .Where(file => !excludedSourcePaths.Any(excludedSourcePath => file.StartsWith(excludedSourcePath)))
      .ToDictionary(file => file, file => FileUtils.GetShootingDate(file, photoExtensions, videoExtensions, wayToGetShootingDateTime));
  }
}
