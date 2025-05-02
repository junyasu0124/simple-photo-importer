namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  public static FileAddress[] GetFilesGroupedByD(string[] sourcePaths, string[] excludedSourcePaths, string[] destPaths, List<(DateTimeOffset Start, DateTimeOffset End)>? dateRanges, HashSet<string> photoExtensions, HashSet<string> videoExtensions, DirectoryNameFormatByD directoryNameFormatByD, string? customDirectoryFormatByD, FileNameFormat fileNameFormat, string? customFileNameFormat, ConflictResolution conflictResolution, ImportOption option, WayToGetShootingDateTime[] wayToGetShootingDateTime, int threadNumber)
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
          if (directoryNameFormatByD == DirectoryNameFormatByD.CustomNameFormat && customDirectoryFormatByD != null)
          {
            directoryPath = Path.Combine(destPaths[destPathsIndex], customDirectoryFormatByD.ToCustomYear(year).ToCustomMonth(month).ToCustomDay(day));
          }
          else
          {
            directoryPath = Path.Combine(destPaths[destPathsIndex], new DateTime(year, month, day).ToString(directoryNameFormatByD switch
            {
              DirectoryNameFormatByD.YMDNoSeparation => "yyyyMMdd",
              DirectoryNameFormatByD.YMDSeparatedByUnderBar => "yyyy_MM_dd",
              DirectoryNameFormatByD.YMDSeparatedByHyphen => "yyyy-MM-dd",
              _ => throw new InvalidOperationException(),
            }));
          }

          var files = dayGroup.ToArray();
          Array.Sort(files, (x, y) => x.Value.CompareTo(y.Value));
          var setDirectory = files.GenerateNewFileNameForEachFile(skippedDirectories, addedNumberDirectories, (directoryPath, files), fileNameFormat, customFileNameFormat, conflictResolution, option).ToList();


          return (DirectoryPath: directoryPath, Files: setDirectory);
        })
        .ToList();

      if (groupedFiles == null)
      {
        continue;
      }
      for (var i = 0; i < skippedDirectories.Count; i++)
      {
        groupedFiles.First(x => x.DirectoryPath == skippedDirectories[i]).Files.Clear();
      }
      for (var j = 0; j < addedNumberDirectories.Count; j++)
      {
        var original = addedNumberDirectories[j].Original;
        var changed = addedNumberDirectories[j].Changed;
        var containedFiles = groupedFiles.First(x => x.DirectoryPath == original).Files;
        for (var k = containedFiles.Count - 1; k >= 0; k--)
        {
          if (containedFiles[k].DestFilePath.StartsWith(original + "\\"))
            containedFiles[k].DestFilePath = containedFiles[k].DestFilePath[original.Length..].Insert(0, changed);
        }
      }

      var foundFiles = groupedFiles.SelectMany(x => x.Files);
      if (foundFiles.Any())
        result.AddRange(foundFiles);
    }

    return [.. result];
  }
}
