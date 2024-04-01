namespace SimplePhotoImporter.Files.GetFiles;

public static partial class GetFiles
{
  private static string GenerateNewFilePath(string directoryPath, string fileName, DateTimeOffset shootingDate, ImportOption option, FileNameFormat fileNameFormat, string? customFileNameFormat)
  {
    string extension;
    if (option.HasFlag(ImportOption.MakeExtensionLower))
      extension = Path.GetExtension(fileName).ToLower();
    else if (option.HasFlag(ImportOption.MakeExtensionUpper))
      extension = Path.GetExtension(fileName).ToUpper();
    else
      extension = Path.GetExtension(fileName);

    if (fileNameFormat == FileNameFormat.CustomNameFormat && customFileNameFormat != null)
      return Path.Combine(directoryPath, $"{shootingDate.ToString(customFileNameFormat).ToCustomFileName(fileName)}{extension}");

    else
      return fileNameFormat switch
      {
        FileNameFormat.OriginalFileName => Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(fileName) + extension),
        FileNameFormat.ShootingDateTimeNoGrouping => Path.Combine(directoryPath, $"{shootingDate:yyyyMMddHHmmss}{extension}"),
        FileNameFormat.ShootingDateTimeGroupedByUnderBar => Path.Combine(directoryPath, $"{shootingDate:yyyy_MM_dd_HH_mm_ss}{extension}"),
        FileNameFormat.ShootingDateTimeGroupedByHyphen => Path.Combine(directoryPath, $"{shootingDate:yyyy-MM-dd-HH-mm-ss}{extension}"),
        _ => throw new InvalidOperationException(),
      };
  }
}
