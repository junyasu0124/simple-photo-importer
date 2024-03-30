using SimplePhotoImporter.Files.Utils;

namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  private static Dictionary<string, DateTimeOffset> GetAllFiles(string[] sourcePaths, string[] excludedSourcePaths, string[] pictureExtensions, string[] movieExtensions, WayToGetShootingDateTime[] wayToGetShootingDateTime)
  {
    string[] extensions = [.. pictureExtensions, .. movieExtensions];

    return sourcePaths
      .Select(sourcePath => Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories))
      .SelectMany(x => x)
      .Where(file => extensions.Contains(Path.GetExtension(file).ToLower()))
      .Where(file => !excludedSourcePaths.Any(excludedSourcePath => file.StartsWith(excludedSourcePath)))
      .ToDictionary(file => file, file => FileUtils.GetShootingDate(file, pictureExtensions, movieExtensions, wayToGetShootingDateTime));
  }
}
