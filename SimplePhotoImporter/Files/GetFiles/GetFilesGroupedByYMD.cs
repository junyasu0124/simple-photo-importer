namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  public static FileAddress[] GetFilesGroupedByYMD(string[] sourcePaths, string[] excludedSourcePaths, string[] destPaths, string[] pictureExtensions, string[] movieExtensions, DirectoryNameFormatByYMD directoryNameFormatByYMD, (string Year, string Month, string Day)? customDirectoryFormatByYMD, FileNameFormat fileNameFormat, string? customFileNameFormat, ConflictResolution conflictResolution, ImportOption option, WayToGetShootingDateTime[] wayToGetShootingDateTime)
  {
    var files = GetAllFiles(sourcePaths, excludedSourcePaths, pictureExtensions, movieExtensions, wayToGetShootingDateTime);

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
         if (directoryNameFormatByYMD == DirectoryNameFormatByYMD.CustomNameFormat && customDirectoryFormatByYMD != null)
           directoryPath = Path.Combine(
             destPaths[destPathsIndex],
             customDirectoryFormatByYMD.Value.Year.ToCustomYear(dayGroup.Key.Year),
             customDirectoryFormatByYMD.Value.Month.ToCustomYear(dayGroup.Key.Year).ToCustomMonth(dayGroup.Key.Month),
             customDirectoryFormatByYMD.Value.Day.ToCustomYear(dayGroup.Key.Year).ToCustomMonth(dayGroup.Key.Month).ToCustomDay(dayGroup.Key.Day)
            );
         else
           directoryPath = Path.Combine(destPaths[destPathsIndex], dayGroup.Key.Year.ToString("0000"), dayGroup.Key.Month.ToString("00"), dayGroup.Key.Day.ToString("00"));

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
