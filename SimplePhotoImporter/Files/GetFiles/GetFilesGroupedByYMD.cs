namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  public static FileAddress[] GetFilesGroupedByYMD(string[] sourcePaths, string[] excludedSourcePaths, string[] destPaths, List<(DateTimeOffset Start, DateTimeOffset End)>? dateRanges, HashSet<string> photoExtensions, HashSet<string> videoExtensions, DirectoryNameFormatByYMD directoryNameFormatByYMD, (string Year, string Month, string Day)? customDirectoryFormatByYMD, FileNameFormat fileNameFormat, string? customFileNameFormat, ConflictResolution conflictResolution, ImportOption option, WayToGetShootingDateTime[] wayToGetShootingDateTime, int threadNumber)
  {
    var files = GetAllFiles(sourcePaths, excludedSourcePaths, dateRanges, photoExtensions, videoExtensions, wayToGetShootingDateTime, threadNumber);

    List<FileAddress> result = [];

    for (var destPathsIndex = 0; destPathsIndex < destPaths.Length; destPathsIndex++)
    {
      List<string> skippedDirectories = [];
      List<(string Original, string Changed)> addedNumberDirectories = [];
      var groupedFiles = files
        .AsParallel()
        .AsOrdered()
        .WithDegreeOfParallelism(threadNumber)
        .GroupBy(file => file.Value.Year << 12 | file.Value.Month << 8 | file.Value.Day)
        .Select(dayGroup =>
        {
          int year = dayGroup.Key >> 12 & 0x3fff;
          int month = dayGroup.Key >> 8 & 0x0f;
          int day = dayGroup.Key & 0xff;
          string? directoryPath;
          if (directoryNameFormatByYMD == DirectoryNameFormatByYMD.CustomNameFormat && customDirectoryFormatByYMD != null)
            directoryPath = Path.Combine(
              destPaths[destPathsIndex],
              customDirectoryFormatByYMD.Value.Year.ToCustomYear(year),
              customDirectoryFormatByYMD.Value.Month.ToCustomYear(year).ToCustomMonth(month),
              customDirectoryFormatByYMD.Value.Day.ToCustomYear(year).ToCustomMonth(month).ToCustomDay(day)
             );
          else
            directoryPath = Path.Combine(destPaths[destPathsIndex], year.ToString("0000"), month.ToString("00"), day.ToString("00"));

          return (DirectoryPath: directoryPath, Files: dayGroup.ToArray());
        })
        .ToDictionary(dayGroup => dayGroup.DirectoryPath,
          dayGroup =>
          {
            var files = dayGroup.Files;
            Array.Sort(files, (x, y) => x.Value.CompareTo(y.Value));
            return files.GenerateNewFileNameForEachFile(skippedDirectories, addedNumberDirectories, dayGroup, fileNameFormat, customFileNameFormat, conflictResolution, option).ToArray();
          });

      if (groupedFiles == null)
      {
        continue;
      }
      for (var i = 0; i < skippedDirectories.Count; i++)
      {
        groupedFiles[skippedDirectories[i]] = [];
      }
      for (var j = 0; j < addedNumberDirectories.Count; j++)
      {
        var original = addedNumberDirectories[j].Original;
        var changed = addedNumberDirectories[j].Changed;
        var containedFiles = groupedFiles[original];
        for (var k = containedFiles.Length - 1; k >= 0; k--)
        {
          if (containedFiles[k].DestFilePath.StartsWith(original + "\\"))
            containedFiles[k].DestFilePath = containedFiles[k].DestFilePath[original.Length..].Insert(0, changed);
        }
      }

      var foundFiles = groupedFiles.SelectMany(x => x.Value);
      if (foundFiles.Any())
        result.AddRange(foundFiles);
    }

    return [.. result];
  }
}
