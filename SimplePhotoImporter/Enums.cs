namespace SimplePhotoImporter;

public enum GroupingMode
{
  NoGrouping,
  GroupedByYMD,
  GroupedByYD,
  GroupedByD,
}

public enum FileNameFormat
{
  OriginalFileName,
  ShootingDateTimeNoGrouping,
  ShootingDateTimeGroupedByUnderBar,
  ShootingDateTimeGroupedByHyphen,
  CustomNameFormat,
}

public enum DirectoryNameFormatByYMD
{
  YMDNoSeparation,
  CustomNameFormat,
}

public enum DirectoryNameFormatByYD
{
  /// <summary>
  /// yyyy\yyyyMMdd
  /// </summary>
  YYMDNoSeparation,
  /// <summary>
  /// yyyy\yyyy_MM_dd
  /// </summary>
  YYMDSeparatedByUnderBar,
  /// <summary>
  /// yyyy\yyyy-MM-dd
  /// </summary>
  YYMDSeparatedByHyphen,
  /// <summary>
  /// yyyy\MMdd
  /// </summary>
  YMDNoSeparation,
  /// <summary>
  /// yyyy\MM_dd
  /// </summary>
  YMDSeparatedByUnderBar,
  /// <summary>
  /// yyyy\MM-dd
  /// </summary>
  YMDSeparatedByHyphen,
  CustomNameFormat,
}

public enum DirectoryNameFormatByD
{
  YMDNoSeparation,
  YMDSeparatedByUnderBar,
  YMDSeparatedByHyphen,
  CustomNameFormat,
}

public enum ConflictResolution
{
  SkipIfSame,
  Skip,
  SkipByDirectory,
  AddNumber,
  AddNumberToDirectory,
}

[Flags]
public enum ImportOption
{
  Move = 1,
  MakeExtensionLower = 2,
  MakeExtensionUpper = 4,
  AddCustomPhotoExtension = 8,
  AddCustomVideoExtension = 16,
  ChangeDateInfoPriority = 32,
  UseASingleThread = 128,
}

[Flags]
public enum FormatSpecifier
{
  Year = 1,
  Month = 2,
  Day = 4,
  Time = 8,
  FileName = 16,
}

public enum WayToGetShootingDateTime
{
  Exif,
  MediaCreated,
  Creation,
  Modified,
  Access,
}
