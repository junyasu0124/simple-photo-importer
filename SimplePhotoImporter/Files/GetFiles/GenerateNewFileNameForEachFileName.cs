using static SimplePhotoImporter.Files.Utils.FileUtils;

namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  public static IEnumerable<FileAddress> GenerateNewFileNameForEachFileName(this KeyValuePair<string, DateTimeOffset>[] files, List<string> skippedDirectories, List<(string Original, string Changed)> addedNumberDirectories, (string DirectoryPath, KeyValuePair<string, DateTimeOffset>[] Files) dayGroup, FileNameFormat fileNameFormat, string? customFileNameFormat, ConflictResolution conflictResolution, ImportOption option)
  {
    return files
      .GroupBy(files => GenerateNewFilePath(dayGroup.DirectoryPath, Path.GetFileName(files.Key), files.Value, option, fileNameFormat, customFileNameFormat))
      .Select(x =>
      {
        var newFilePath = x.Key;
        var files = x.ToArray();

        if (skippedDirectories.Contains(dayGroup.DirectoryPath))
          return [];
        if (addedNumberDirectories.Any(x => x.Original == dayGroup.DirectoryPath))
        {
          var changed = addedNumberDirectories.First(x => x.Original == dayGroup.DirectoryPath).Changed;
          newFilePath = Path.Combine(changed, Path.GetFileName(newFilePath));
        }

        if (File.Exists(newFilePath))
        {
          var i = 2;
          if (conflictResolution == ConflictResolution.SkipIfSame)
          {
            return files.Where(file => !IsSameFile(file.Key, newFilePath))
            .Select(file =>
            {
              while (true)
              {
                var renewedFilePath = Path.Combine(Path.GetDirectoryName(newFilePath) ?? "", $"{Path.GetFileNameWithoutExtension(newFilePath)} ({i++}){Path.GetExtension(newFilePath)}");
                if (File.Exists(renewedFilePath))
                  continue;
                else
                  return new FileAddress(file.Key, renewedFilePath, file.Value);
              }
            });
          }
          else if (conflictResolution == ConflictResolution.AddNumber)
          {
            return files.Select(file =>
            {
              while (true)
              {
                var renewedFilePath = Path.Combine(Path.GetDirectoryName(newFilePath) ?? "", $"{Path.GetFileNameWithoutExtension(newFilePath)} ({i++}){Path.GetExtension(newFilePath)}");
                if (File.Exists(renewedFilePath))
                  continue;
                else
                  return new FileAddress(file.Key, renewedFilePath, file.Value);
              }
            });
          }
          else if (conflictResolution == ConflictResolution.Skip)
          {
            return [];
          }
          else if (conflictResolution == ConflictResolution.AddNumberToDirectory)
          {
            var directoryPath = Path.GetDirectoryName(newFilePath) ?? "";
            string newDirectoryPath;
            while (true)
            {
              var renewedDirectoryPath = directoryPath + $" ({i++})";
              if (Directory.Exists(renewedDirectoryPath))
                continue;
              else
              {
                newDirectoryPath = renewedDirectoryPath;
                break;
              }
            }
            addedNumberDirectories.Add((directoryPath, newDirectoryPath));
            List<string> filePaths = [];
            i = 2;
            foreach (var file in files)
            {
              var fileName = Path.GetFileName(newFilePath);
              var renewedFilePath = Path.Combine(newDirectoryPath, fileName);
              if (filePaths.Contains(renewedFilePath))
                while (true)
                {
                  var renewedFilePath2 = Path.Combine(newDirectoryPath, $"{Path.GetFileNameWithoutExtension(fileName)} ({i++}){Path.GetExtension(fileName)}");
                  if (File.Exists(renewedFilePath2))
                    continue;
                  else
                  {
                    filePaths.Add(renewedFilePath2);
                    break;
                  }
                }
              else
                filePaths.Add(renewedFilePath);
            }
            return files.Zip(filePaths, (file, filePath) => new FileAddress(file.Key, filePath, file.Value));
          }
          else if (conflictResolution == ConflictResolution.SkipByDirectory)
          {
            skippedDirectories.Add(dayGroup.DirectoryPath);
            return [];
          }
          else
            throw new InvalidOperationException();
        }
        else
        {
          return files.Length switch
          {
            0 => [],
            1 => files.Select(file => new FileAddress(file.Key, newFilePath, file.Value)),
            _ => AddNumber(),
          };

          IEnumerable<FileAddress> AddNumber()
          {
            var i = 1;
            return files.Select(file =>
            {
              if (i == 1)
              {
                i++;
                return new FileAddress(file.Key, newFilePath, file.Value);
              }

              while (true)
              {
                var renewedFilePath = Path.Combine(Path.GetDirectoryName(newFilePath) ?? "", $"{Path.GetFileNameWithoutExtension(newFilePath)} ({i++}){Path.GetExtension(newFilePath)}");
                if (File.Exists(renewedFilePath))
                  continue;
                else
                  return new FileAddress(file.Key, renewedFilePath, file.Value);
              }
            });
          }
        }
      })
      .SelectMany(x => x);
  }
}
