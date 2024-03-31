namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  public static FileAddress[] GetFilesGroupedByD(string[] sourcePaths, string[] excludedSourcePaths, string[] destPaths, string[] photoExtensions, string[] videoExtensions, DirectoryNameFormatByD directoryNameFormatByD, string? customDirectoryFormatByD, FileNameFormat fileNameFormat, string? customFileNameFormat, ConflictResolution conflictResolution, ImportOption option, WayToGetShootingDateTime[] wayToGetShootingDateTime)
  {
    var files = GetAllFiles(sourcePaths, excludedSourcePaths, photoExtensions, videoExtensions, wayToGetShootingDateTime);

    List<FileAddress> result = [];

    for (var destPathIndex = 0; destPathIndex < destPaths.Length; destPathIndex++)
    {
      List<string> skippedDirectories = [];
      List<(string Original, string Changed)> addedNumberDirectories = [];
      var groupedFiles = files
       .GroupBy(file => new DateOnly(file.Value.Year, file.Value.Month, file.Value.Day))
       .Select(dayGroup =>
       {
         string? directoryPath;
         if (directoryNameFormatByD == DirectoryNameFormatByD.CustomNameFormat && customDirectoryFormatByD != null)
         {
           directoryPath = Path.Combine(destPaths[destPathIndex], customDirectoryFormatByD.ToCustomYear(dayGroup.Key.Year).ToCustomMonth(dayGroup.Key.Month).ToCustomDay(dayGroup.Key.Day));
         }
         else
           directoryPath = Path.Combine(destPaths[destPathIndex], dayGroup.Key.ToString(directoryNameFormatByD switch
           {
             DirectoryNameFormatByD.YMDNoSeparation => "yyyyMMdd",
             DirectoryNameFormatByD.YMDSeparatedByUnderBar => "yyyy_MM_dd",
             DirectoryNameFormatByD.YMDSeparatedByHyphen => "yyyy-MM-dd",
             _ => throw new InvalidOperationException(),
           }));
         return (DirectoryPath: directoryPath, Files: dayGroup.ToArray());
       })
       .Select(dayGroup =>
       {
         var files = dayGroup.Files;
         Array.Sort(files, (x, y) => x.Value.CompareTo(y.Value));
         var setDirectory = files.GenerateNewFileNameForEachFileName(skippedDirectories, addedNumberDirectories, dayGroup, fileNameFormat, customFileNameFormat, conflictResolution, option).ToList();
         return (dayGroup.DirectoryPath, Files: setDirectory);
       })
       .ToList();

      if (groupedFiles == null)
      {
        continue;
      }
      if (skippedDirectories.Count > 0)
      {
        for (var i = 0; i < skippedDirectories.Count; i++)
        {
          groupedFiles.First(x => x.DirectoryPath == skippedDirectories[i]).Files.Clear();
        }
      }
      if (addedNumberDirectories.Count > 0)
      {
        for (var i = 0; i < addedNumberDirectories.Count; i++)
        {
          var original = addedNumberDirectories[i].Original;
          var changed = addedNumberDirectories[i].Changed;
          var containedFiles = groupedFiles.First(x => x.DirectoryPath == original).Files;
          for (var j = containedFiles.Count - 1; j >= 0; j--)
          {
            if (containedFiles[j].DestFilePath.StartsWith(original + "\\"))
              containedFiles[j].DestFilePath = containedFiles[j].DestFilePath[original.Length..].Insert(0, changed);
          }
        }
      }

      var foundFiles = groupedFiles.SelectMany(x => x.Files);
      if (foundFiles.Any())
        result.AddRange(foundFiles);
    }

    return [.. result];
  }
}
