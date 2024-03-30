﻿using static SimplePhotoImporter.Files.Utils.FileUtils;

namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  public static FileAddress[] GetFilesNoGrouping(string[] sourcePaths, string[] excludedSourcePaths, string[] destPaths, string[] pictureExtensions, string[] movieExtensions, FileNameFormat fileNameFormat, string? customFileNameFormat, ConflictResolution conflictResolution, ImportOption option, WayToGetShootingDateTime[] wayToGetShootingDateTime)
  {
    var files = GetAllFiles(sourcePaths, excludedSourcePaths, pictureExtensions, movieExtensions, wayToGetShootingDateTime);

    List<FileAddress> result = [];

    for (var destPathsIndex = 0; destPathsIndex < destPaths.Length; destPathsIndex++)
    {
      List<(string SourcePath, string NewFilePath)> willBeAddedFilePath = [];
      var isSkipped = false;
      FileAddress[] collisionAvoidance = files
        .OrderBy(file => file.Value)
        .Select(file =>
        {
          if (isSkipped)
            return null;

          var fileName = Path.GetFileName(file.Key);
          var newFilePath = GenerateNewFilePath(destPaths[destPathsIndex], fileName, file.Value, option, fileNameFormat, customFileNameFormat);

          var i = 2;
          if (willBeAddedFilePath.Any(x => x.NewFilePath == newFilePath))
          {
            while (true)
            {
              var renewedFilePath = Path.Combine(Path.GetDirectoryName(newFilePath) ?? "", $"{Path.GetFileNameWithoutExtension(newFilePath)} ({i++}){Path.GetExtension(newFilePath)}");
              if (willBeAddedFilePath.Any(x => x.NewFilePath == renewedFilePath))
                continue;
              else
              {
                willBeAddedFilePath.Add((file.Key, renewedFilePath));
                return new FileAddress(file.Key, renewedFilePath, file.Value);
              }
            }
          }
          if (File.Exists(newFilePath))
          {
            if (conflictResolution == ConflictResolution.SkipIfSame)
            {
              if (IsSameFile(file.Key, newFilePath))
              {
                return null;
              }

              while (true)
              {
                var renewedFilePath = Path.Combine(Path.GetDirectoryName(newFilePath) ?? "", $"{Path.GetFileNameWithoutExtension(newFilePath)} ({i++}){Path.GetExtension(newFilePath)}");
                if (File.Exists(renewedFilePath) || willBeAddedFilePath.Any(x => x.NewFilePath == renewedFilePath))
                  continue;
                else
                {
                  willBeAddedFilePath.Add((file.Key, renewedFilePath));
                  return new FileAddress(file.Key, renewedFilePath, file.Value);
                }
              }
            }
            else if (conflictResolution == ConflictResolution.AddNumber)
            {
              while (true)
              {
                var renewedFilePath = Path.Combine(Path.GetDirectoryName(newFilePath) ?? "", $"{Path.GetFileNameWithoutExtension(newFilePath)} ({i++}){Path.GetExtension(newFilePath)}");
                if (File.Exists(renewedFilePath) || willBeAddedFilePath.Any(x => x.NewFilePath == renewedFilePath))
                  continue;
                else
                {
                  willBeAddedFilePath.Add((file.Key, renewedFilePath));
                  return new FileAddress(file.Key, renewedFilePath, file.Value);
                }
              }
            }
            else if (conflictResolution == ConflictResolution.Skip)
            {
              isSkipped = true;
              return null;
            }
            else if (conflictResolution == ConflictResolution.SkipByDirectory)
            {
              isSkipped = true;
              return null;
            }
            else
              throw new InvalidOperationException();
          }
          else
          {
            willBeAddedFilePath.Add((file.Key, newFilePath));
            return new FileAddress(file.Key, newFilePath, file.Value);
          }
        })
        .Where(x => x != null)
        .ToArray() as FileAddress[] ?? [];

      if (isSkipped)
      {
        continue;
      }

      if (collisionAvoidance.Length > 0)
        result.AddRange(collisionAvoidance);
    }

    return [.. result];
  }
}