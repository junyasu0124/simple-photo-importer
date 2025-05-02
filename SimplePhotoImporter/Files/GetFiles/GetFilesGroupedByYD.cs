namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  public static FileAddress[] GetFilesGroupedByYD(string[] sourcePaths, string[] excludedSourcePaths, string[] destPaths, List<(DateTimeOffset Start, DateTimeOffset End)>? dateRanges, HashSet<string> photoExtensions, HashSet<string> videoExtensions, DirectoryNameFormatByYD directoryNameFormatByYD, (string Year, string Day)? customDirectoryFormatByYD, FileNameFormat fileNameFormat, string? customFileNameFormat, ConflictResolution conflictResolution, ImportOption option, WayToGetShootingDateTime[] wayToGetShootingDateTime, int threadNumber)
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
          if (directoryNameFormatByYD == DirectoryNameFormatByYD.CustomNameFormat && customDirectoryFormatByYD != null)
          {
            directoryPath = Path.Combine(
              destPaths[destPathsIndex],
              customDirectoryFormatByYD.Value.Year.ToCustomYear(year),
              customDirectoryFormatByYD.Value.Day.ToCustomYear(year).ToCustomMonth(month).ToCustomDay(day)
            );
          }
          else
          {
            directoryPath = directoryNameFormatByYD switch
            {
              DirectoryNameFormatByYD.YYMDNoSeparation => Path.Combine(destPaths[destPathsIndex], $"{year:0000}", $"{year:0000}{month:00}{day:00}"),
              DirectoryNameFormatByYD.YYMDSeparatedByUnderBar => Path.Combine(destPaths[destPathsIndex], $"{year:0000}", $"{year:0000}_{month:00}_{day:00}"),
              DirectoryNameFormatByYD.YYMDSeparatedByHyphen => Path.Combine(destPaths[destPathsIndex], $"{year:0000}", $"{year:0000}-{month:00}-{day:00}"),
              DirectoryNameFormatByYD.YMDNoSeparation => Path.Combine(destPaths[destPathsIndex], $"{year:0000}", $"{month:00}{day:00}"),
              DirectoryNameFormatByYD.YMDSeparatedByUnderBar => Path.Combine(destPaths[destPathsIndex], $"{year:0000}", $"{month:00}_{day:00}"),
              DirectoryNameFormatByYD.YMDSeparatedByHyphen => Path.Combine(destPaths[destPathsIndex], $"{year:0000}", $"{month:00}-{day:00}"),
              _ => throw new InvalidOperationException(),
            };
          }

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
