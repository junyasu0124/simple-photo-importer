namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  public static FileAddress[] GetFilesGroupedByYD(string[] sourcePaths, string[] excludedSourcePaths, string[] destPaths, string[] photoExtensions, string[] videoExtensions, DirectoryNameFormatByYD directoryNameFormatByYD, (string Year, string Day)? customDirectoryFormatByYD, FileNameFormat fileNameFormat, string? customFileNameFormat, ConflictResolution conflictResolution, ImportOption option, WayToGetShootingDateTime[] wayToGetShootingDateTime)
  {
    var files = GetAllFiles(sourcePaths, excludedSourcePaths, photoExtensions, videoExtensions, wayToGetShootingDateTime);

    List<FileAddress> result = [];

    for (var destPathsIndex = 0; destPathsIndex < destPaths.Length; destPathsIndex++)
    {
      List<string> skippedDirectories = [];
      List<(string Original, string Changed)> addedNumberDirectories = [];
      var groupedFiles = files
       .GroupBy(file => new DateOnly(file.Value.Year, file.Value.Month, file.Value.Day))
       .Select(dayGroup =>
       {
         string? directoryPath;
         if (directoryNameFormatByYD == DirectoryNameFormatByYD.CustomNameFormat && customDirectoryFormatByYD != null)
           directoryPath = Path.Combine(
             destPaths[destPathsIndex],
             customDirectoryFormatByYD.Value.Year.ToCustomYear(dayGroup.Key.Year),
             customDirectoryFormatByYD.Value.Day.ToCustomYear(dayGroup.Key.Year).ToCustomMonth(dayGroup.Key.Month).ToCustomDay(dayGroup.Key.Day)
            );
         else
         {
           directoryPath = directoryNameFormatByYD switch
           {
             DirectoryNameFormatByYD.YYMDNoSeparation => Path.Combine(destPaths[destPathsIndex], $"{dayGroup.Key.Year:0000}", $"{dayGroup.Key.Year:0000}{dayGroup.Key.Month:00}{dayGroup.Key.Day:00}"),
             DirectoryNameFormatByYD.YYMDSeparatedByUnderBar => Path.Combine(destPaths[destPathsIndex], $"{dayGroup.Key.Year:0000}", $"{dayGroup.Key.Year:0000}_{dayGroup.Key.Month:00}_{dayGroup.Key.Day:00}"),
             DirectoryNameFormatByYD.YYMDSeparatedByHyphen => Path.Combine(destPaths[destPathsIndex], $"{dayGroup.Key.Year:0000}", $"{dayGroup.Key.Year:0000}-{dayGroup.Key.Month:00}-{dayGroup.Key.Day:00}"),
             DirectoryNameFormatByYD.YMDNoSeparation => Path.Combine(destPaths[destPathsIndex], $"{dayGroup.Key.Year:0000}", $"{dayGroup.Key.Month:00}{dayGroup.Key.Day:00}"),
             DirectoryNameFormatByYD.YMDSeparatedByUnderBar => Path.Combine(destPaths[destPathsIndex], $"{dayGroup.Key.Year:0000}", $"{dayGroup.Key.Month:00}_{dayGroup.Key.Day:00}"),
             DirectoryNameFormatByYD.YMDSeparatedByHyphen => Path.Combine(destPaths[destPathsIndex], $"{dayGroup.Key.Year:0000}", $"{dayGroup.Key.Month:00}-{dayGroup.Key.Day:00}"),
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
           return files.GenerateNewFileNameForEachFileName(skippedDirectories, addedNumberDirectories, dayGroup, fileNameFormat, customFileNameFormat, conflictResolution, option).ToArray();
         });

      if (groupedFiles == null)
      {
        continue;
      }
      if (skippedDirectories.Count > 0)
      {
        for (var i = 0; i < skippedDirectories.Count; i++)
        {
          groupedFiles[skippedDirectories[i]] = [];
        }
      }
      if (addedNumberDirectories.Count > 0)
      {
        for (var i = 0; i < addedNumberDirectories.Count; i++)
        {
          var original = addedNumberDirectories[i].Original;
          var changed = addedNumberDirectories[i].Changed;
          var containedFiles = groupedFiles[original];
          for (var j = containedFiles.Length - 1; j >= 0; j--)
          {
            if (containedFiles[j].DestFilePath.StartsWith(original + "\\"))
              containedFiles[j].DestFilePath = containedFiles[j].DestFilePath[original.Length..].Insert(0, changed);
          }
        }
      }

      var foundFiles = groupedFiles.SelectMany(x => x.Value);
      if (foundFiles.Any())
        result.AddRange(foundFiles);
    }

    return [.. result];
  }
}
