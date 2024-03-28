using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable CA1416 // プラットフォームの互換性を検証

START:

string[] pictureExtensions = [".jpeg", ".jpg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".ico", ".svg", ".heic", ".mov", ".mp4", ".m2ts", ".mts", ".avi", ".flv"];
string[] movieExtensions = [".mov", ".mp4", ".m2ts", ".mts", ".avi", ".flv"];


Console.WriteLine("Simple Photo Importer");

if (!OperatingSystem.IsWindows())
{
  Console.Error.WriteLine("This program is only supported on Windows.");
  return;
}

string sourcePath;
while (true)
{
  Console.WriteLine("Enter the path to the folder you want to import from:");
  var inputFromPath = Console.ReadLine();

  if (inputFromPath == null || !Directory.Exists(inputFromPath))
    Console.Error.WriteLine("The specified path does not exist.");
  else
  {
    sourcePath = inputFromPath;
    break;
  }
}

string destPath;
while (true)
{
  Console.WriteLine("Enter the path to the folder you want to import to:");
  var inputToPath = Console.ReadLine();

  if (inputToPath == null || !Directory.Exists(inputToPath))
    Console.Error.WriteLine("The specified path does not exist.");
  else
  {
    destPath = inputToPath;
    break;
  }
}

SeparateMode separateMode;
while (true)
{
  Console.WriteLine("Enter the mode to separate the files:");
  Console.WriteLine("1: By none");
  Console.WriteLine("2: By year, month, and day");
  Console.WriteLine("3: By day");
  var inputSeparateMode = Console.ReadLine();

  if (inputSeparateMode == "1")
  {
    separateMode = SeparateMode.ByNone;
    break;
  }
  else if (inputSeparateMode == "2")
  {
    separateMode = SeparateMode.ByYMD;
    break;
  }
  else if (inputSeparateMode == "3")
  {
    separateMode = SeparateMode.ByD;
    break;
  }
  else if (inputSeparateMode == null)
  {
    continue;
  }
  else
  {
    Console.Error.WriteLine("Invalid input.");
  }
}

(FileNameFormat file, DirectoryNameFormatByYMD directoryByYMD, DirectoryNameFormatByD directoryByD) formatRule = (0, 0, 0);
string? customFileFormat = null;
(string year, string month, string day)? customDirectoryFormatByYMD = null;
string? customDirectoryFormatByD = null;
while (true)
{
  Console.WriteLine("Enter the format of the file name:");
  Console.WriteLine("1: Original file name");
  Console.WriteLine("2: Shooting date time as yyyyMMddHHmmss");
  Console.WriteLine("3: Shooting date time as yyyy_MM_dd_HH_mm_ss");
  Console.WriteLine("4: Shooting date time as yyyy-MM-dd-HH-mm-ss");
  Console.WriteLine("5: Custom format");
  var inputFileNameFormat = Console.ReadLine();
  if (string.IsNullOrWhiteSpace(inputFileNameFormat))
    continue;
  switch (inputFileNameFormat)
  {
    case "1":
      formatRule.file = FileNameFormat.OriginalFileName;
      break;
    case "2":
      formatRule.file = FileNameFormat.ShootingDateTimeNoSeparation;
      break;
    case "3":
      formatRule.file = FileNameFormat.ShootingDateTimeSeparatedByUnderBar;
      break;
    case "4":
      formatRule.file = FileNameFormat.ShootingDateTimeSeparatedByHyphen;
      break;
    case "5":
      formatRule.file = FileNameFormat.CustomFormat;
      customFileFormat = SetCustomFormat.SetCustomFileFormat();
      if (customFileFormat == null)
        return;
      break;
    default:
      Console.Error.WriteLine("Invalid input.");
      continue;
  }
  break;
}
if (separateMode == SeparateMode.ByYMD)
{
  while (true)
  {
    Console.WriteLine("Enter the format of the directory name:");
    Console.WriteLine("1: Year\\Month\\Day as yyyy\\MM\\dd");
    Console.WriteLine("2: Custom format");
    var inputDirectoryNameFormatByYMD = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(inputDirectoryNameFormatByYMD))
      continue;
    switch (inputDirectoryNameFormatByYMD)
    {
      case "1":
        formatRule.directoryByYMD = DirectoryNameFormatByYMD.YMD;
        break;
      case "2":
        formatRule.directoryByYMD = DirectoryNameFormatByYMD.CustomFormat;
        customDirectoryFormatByYMD = SetCustomFormat.SetCustomDirectoryFormatByYearMonthAndDay();
        if (customDirectoryFormatByYMD == null)
          return;
        break;
      default:
        Console.Error.WriteLine("Invalid input.");
        continue;
    }
    break;
  }
}
if (separateMode == SeparateMode.ByD)
{
  while (true)
  {
    Console.WriteLine("Enter the format of the directory name:");
    Console.WriteLine("1: YearMonthDay as yyyyMMdd");
    Console.WriteLine("2: Year_Month_Day as yyyy_MM_dd");
    Console.WriteLine("3: Year-Month-Day as yyyy-MM-dd");
    Console.WriteLine("4: Custom format");
    var inputDirectoryNameFormatByD = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(inputDirectoryNameFormatByD))
      continue;
    switch (inputDirectoryNameFormatByD)
    {
      case "1":
        formatRule.directoryByD = DirectoryNameFormatByD.YMDNoSeparation;
        break;
      case "2":
        formatRule.directoryByD = DirectoryNameFormatByD.YMDSeparatedByUnderBar;
        break;
      case "3":
        formatRule.directoryByD = DirectoryNameFormatByD.YMDSeparatedByHyphen;
        break;
      case "4":
        formatRule.directoryByD = DirectoryNameFormatByD.CustomFormat;
        customDirectoryFormatByD = SetCustomFormat.SetCustomDirectoryFormatByDay();
        if (customDirectoryFormatByD == null)
          return;
        break;
      default:
        Console.Error.WriteLine("Invalid input.");
        continue;
    }
    break;
  }
}

ImportOption option = 0;
while (true)
{
  Console.WriteLine("Enter the options you want to enable. You can enter multiple options by separating them with a space:");
  Console.WriteLine("1: Move instead of copy");
  Console.WriteLine("2: If the same name file already exists, check if the files are the same and skip if they are the same, otherwise, add a number to the file name. If this option is not enabled, always add a number to the file name.");
  Console.WriteLine("3: Make the extension lower case");
  Console.WriteLine("4: Make the extension upper case");
  Console.WriteLine("5: Add custom picture extension");
  Console.WriteLine("6: Add custom movie extension");
  Console.WriteLine("7: Change the way to get shooting date time");
  Console.WriteLine("8: Use a single thread");
  Console.WriteLine("Enter nothing to finish.");
  var inputOption = Console.ReadLine();

  if (string.IsNullOrWhiteSpace(inputOption))
    break;
  var options = inputOption.Split(' ');
  option = 0;
  var invalid = false;
  if (options.Length != options.Distinct().Count())
  {
    Console.Error.WriteLine("You can't specify the same option multiple times.");
  }
  foreach (var opt in options)
  {
    if (opt == "1")
      option |= ImportOption.Move;
    else if (opt == "2")
      option |= ImportOption.SkipIfSame;
    else if (opt == "3")
      option |= ImportOption.MakeExtensionLower;
    else if (opt == "4")
      option |= ImportOption.MakeExtensionUpper;
    else if (opt == "5")
      option |= ImportOption.AddCustomPictureExtension;
    else if (opt == "6")
      option |= ImportOption.AddCustomMovieExtension;
    else if (opt == "7")
      option |= ImportOption.ChangeWayToGetShootingDateTime;
    else if (opt == "8")
      option |= ImportOption.UseASingleThread;
    else
    {
      Console.Error.WriteLine("Invalid input.");
      invalid = true;
      break;
    }
  }
  if (invalid)
    continue;
  if (option.HasFlag(ImportOption.MakeExtensionLower) && option.HasFlag(ImportOption.MakeExtensionUpper))
  {
    Console.Error.WriteLine("You can't specify both 3 and 4.");
    continue;
  }
  break;
}

if (option.HasFlag(ImportOption.AddCustomPictureExtension))
{
  Console.WriteLine("""Enter the custom picture extensions you want to add. You can enter multiple extensions by separating them with a space. If you want to remove the default picture extensions, enter ":remove" and the custom extensions you want to use.""");
  Console.WriteLine($"Default picture extensions: {string.Join(' ', pictureExtensions)}");
  var customPictureExtension = Console.ReadLine();
  if (customPictureExtension != null)
  {
    var entereds = customPictureExtension.Split(' ').ToList();
    if (entereds.Contains(":remove"))
    {
      pictureExtensions = [];
      entereds.Remove(":remove");
    }
    var customPictureExtensions = entereds.Select(x => x.ToLower()).Select(x => x.StartsWith('.') ? x : "." + x);
    pictureExtensions = [.. pictureExtensions, .. customPictureExtensions];
  }
}
if (option.HasFlag(ImportOption.AddCustomMovieExtension))
{
  Console.WriteLine("""Enter the custom movie extensions you want to add. You can enter multiple extensions by separating them with a space. If you want to remove the default movie extensions, enter ":remove" and the custom extensions you want to use.""");
  Console.WriteLine($"Default movie extensions: {string.Join(' ', movieExtensions)}");
  var customMovieExtension = Console.ReadLine();
  if (customMovieExtension != null)
  {
    var entereds = customMovieExtension.Split(' ').ToList();
    if (entereds.Contains(":remove"))
    {
      movieExtensions = [];
      entereds.Remove(":remove");
    }
    var customMovieExtensions = entereds.Select(x => x.ToLower()).Select(x => x.StartsWith('.') ? x : "." + x);
    movieExtensions = [.. movieExtensions, .. customMovieExtensions];
  }
}

var threadCount = option.HasFlag(ImportOption.UseASingleThread) ? 1 : Environment.ProcessorCount;
var startTime = DateTimeOffset.Now;

if (separateMode == SeparateMode.ByNone)
{
  var files = GetFiles(sourcePath);

  var progress = new Progress(50, files.Count);
  files.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(file =>
  {
    var fileName = Path.GetFileName(file.Key);
    var newFilePath = GenerateNewFilePath(destPath, fileName, file.Value, option);
    CopyFile(file.Key, newFilePath, file.Value, option);
    lock (progress)
      progress.Update("");
  });
  progress.Done("Done");
}
else if (separateMode == SeparateMode.ByYMD)
{
  var files = GetFilesSeparatedByYearAndMonthAndDay(sourcePath);

  var progress = new Progress(50, files.filesCount);
  files.files.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(year =>
  {
    year.Value.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(month =>
    {
      month.Value.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(day =>
      {
        string? directoryPath;
        if (formatRule.directoryByYMD == DirectoryNameFormatByYMD.CustomFormat && customDirectoryFormatByYMD != null)
          directoryPath = Path.Combine(destPath, year.Key.ToCustomYear(customDirectoryFormatByYMD.Value.year), month.Key.ToCustomYear(customDirectoryFormatByYMD.Value.month), day.Key.ToCustomYear(customDirectoryFormatByYMD.Value.day));
        else
          directoryPath = Path.Combine(destPath, year.Key.ToString(), month.Key.ToString(), day.Key.ToString());

        Directory.CreateDirectory(directoryPath);

        day.Value.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(file =>
        {
          var fileName = Path.GetFileName(file.path);
          var newFilePath = Path.Combine(directoryPath, fileName);
          CopyFile(file.path, newFilePath, file.date, option);
          lock (progress)
            progress.Update("");
        });
      });
    });
  });
  progress.Done("Done");
}
else if (separateMode == SeparateMode.ByD)
{
  var files = GetFilesSeparatedByDay(sourcePath);

  var progress = new Progress(50, files.filesCount);
  files.files.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(day =>
  {
    string? directoryPath;
    if (formatRule.directoryByD == DirectoryNameFormatByD.CustomFormat && customDirectoryFormatByD != null)
      directoryPath = Path.Combine(destPath, CustomFormatterExtension.ToCustomYear(day.Key.Year, customDirectoryFormatByD), CustomFormatterExtension.ToCustomMonth(day.Key.Month, customDirectoryFormatByD), CustomFormatterExtension.ToCustomDay(day.Key.Day, customDirectoryFormatByD));
    else
      directoryPath = Path.Combine(destPath, day.Key.ToString(formatRule.directoryByD switch
      {
        DirectoryNameFormatByD.YMDNoSeparation => "yyyyMMdd",
        DirectoryNameFormatByD.YMDSeparatedByUnderBar => "yyyy_MM_dd",
        DirectoryNameFormatByD.YMDSeparatedByHyphen => "yyyy-MM-dd",
        _ => throw new InvalidOperationException(),
      }));

    Directory.CreateDirectory(directoryPath);

    day.Value.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(file =>
    {
      var fileName = Path.GetFileName(file.path);
      var newFilePath = GenerateNewFilePath(directoryPath, fileName, file.date, option);
      CopyFile(file.path, newFilePath, file.date, option);
      lock (progress)
        progress.Update("");
    });
  });
  progress.Done("Done");
}
Console.WriteLine($"Elapsed time: {Math.Floor((DateTimeOffset.Now - startTime).TotalMilliseconds)} ms");

goto START;

void CopyFile(string sourceFileName, string destFileName, DateTimeOffset shootingDate, ImportOption option)
{
  if (File.Exists(destFileName))
  {
    Retry();
    return;
  }
  try
  {
    if (option.HasFlag(ImportOption.Move))
      File.Move(sourceFileName, destFileName, false);
    else
      File.Copy(sourceFileName, destFileName, false);
  }
  catch (IOException)
  {
    Retry();
  }
  catch (Exception e)
  {
    Console.Error.WriteLine($"Failed to copy {sourceFileName} to {destFileName}: {e.Message}");
  }

  void Retry()
  {
    if (option.HasFlag(ImportOption.SkipIfSame) && IsSameFile(sourcePath, destPath))
    {
      return;
    }
    var i = 2;
    while (true)
    {
      destFileName = GenerateNewFilePath(Path.GetDirectoryName(destFileName) ?? "", $"{Path.GetFileNameWithoutExtension(destFileName)} ({i}){Path.GetExtension(destFileName)}", shootingDate, option);
      try
      {
        if (option.HasFlag(ImportOption.Move))
          File.Move(sourceFileName, destFileName, false);
        else
          File.Copy(sourceFileName, destFileName, false);
        break;
      }
      catch (IOException)
      {
        if (i > 1000)
        {
          Console.Error.WriteLine($"Failed to copy {sourceFileName} to {destFileName}: Too many conflicts.");
          break;
        }
        i++;
      }
      catch (Exception e)
      {
        Console.Error.WriteLine($"Failed to copy {sourceFileName} to {destFileName}: {e.Message}");
        break;
      }
    }
  }
}

(int filesCount, Dictionary<int, Dictionary<int, Dictionary<int, (string path, DateTimeOffset date)[]>>> files) GetFilesSeparatedByYearAndMonthAndDay(string parentDirectory)
{
  var files = GetFiles(parentDirectory);

  return (files.Count,
    files
    .GroupBy(file => file.Value.Year)
    .ToDictionary(yearGroup => yearGroup.Key,
      yearGroup => yearGroup
        .GroupBy(file => file.Value.Month)
        .ToDictionary(monthGroup => monthGroup.Key,
          monthGroup => monthGroup.GroupBy(file => file.Value.Day)
            .ToDictionary(dayGroup => dayGroup.Key,
              dayGroup => dayGroup.Select(file => (file.Key, file.Value)).ToArray()))));
}
(int filesCount, Dictionary<DateOnly, (string path, DateTimeOffset date)[]> files) GetFilesSeparatedByDay(string parentDirectory)
{
  var files = GetFiles(parentDirectory);

  return (files.Count,
    files
    .GroupBy(file => new DateOnly(file.Value.Year, file.Value.Month, file.Value.Day))
    .ToDictionary(dayGroup => dayGroup.Key,
      dayGroup => dayGroup.Select(file => (file.Key, file.Value)).ToArray()));
}
Dictionary<string, DateTimeOffset> GetFiles(string parentDirectory)
{
  string[] extensions = [.. pictureExtensions, .. movieExtensions];

  return Directory.EnumerateFiles(parentDirectory, "*.*", SearchOption.AllDirectories)
      .Where(file => extensions.Contains(Path.GetExtension(file).ToLower()))
      .ToDictionary(file => file, GetShootingDate);
}
DateTimeOffset GetShootingDate(string filePath)
{
  try
  {
    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    using var img = Image.FromStream(fs, false, false);
    var item = img.GetPropertyItem(36867);
    if (item == null || item.Value == null || item.Value.Length == 0)
      return new DateTimeOffset(File.GetCreationTime(filePath));
    var picDateStr = Encoding.UTF8.GetString(item.Value).Trim(['\0']);
    return DateTimeOffset.ParseExact(picDateStr, "yyyy:MM:dd HH:mm:ss", null);
  }
  catch { }
  return new DateTimeOffset(File.GetCreationTime(filePath));
}
[MethodImpl(MethodImplOptions.AggressiveInlining)]
string GenerateNewFilePath(string directoryPath, string originalFileName, DateTimeOffset shootingDate, ImportOption option)
{
  string extension;
  if (option.HasFlag(ImportOption.MakeExtensionLower))
    extension = Path.GetExtension(originalFileName).ToLower();
  else if (option.HasFlag(ImportOption.MakeExtensionUpper))
    extension = Path.GetExtension(originalFileName).ToUpper();
  else
    extension = Path.GetExtension(originalFileName);

  if (formatRule.file == FileNameFormat.CustomFormat && customFileFormat != null)
    return Path.Combine(directoryPath, $"{shootingDate.ToString(customFileFormat).ToCustomFileName(originalFileName)}{extension}");

  else
    return formatRule.file switch
    {
      FileNameFormat.OriginalFileName => Path.Combine(directoryPath, originalFileName),
      FileNameFormat.ShootingDateTimeNoSeparation => Path.Combine(directoryPath, $"{shootingDate:yyyyMMddHHmmss}{extension}"),
      FileNameFormat.ShootingDateTimeSeparatedByUnderBar => Path.Combine(directoryPath, $"{shootingDate:yyyy_MM_dd_HH_mm_ss}{extension}"),
      FileNameFormat.ShootingDateTimeSeparatedByHyphen => Path.Combine(directoryPath, $"{shootingDate:yyyy-MM-dd-HH-mm-ss}{extension}"),
      _ => throw new InvalidOperationException(),
    };
}
bool IsSameFile(string file1, string file2)
{
  byte[] hash1 = ComputeFileHash(file1);
  byte[] hash2 = ComputeFileHash(file2);
  return CompareHashes(hash1, hash2);

  byte[] ComputeFileHash(string filePath)
  {
    using var md5 = MD5.Create();
    using var stream = File.OpenRead(filePath);
    return md5.ComputeHash(stream);
  }
  bool CompareHashes(byte[] hash1, byte[] hash2)
  {
    if (hash1.Length != hash2.Length)
      return false;
    for (int i = 0; i < hash1.Length; i++)
      if (hash1[i] != hash2[i])
        return false;
    return true;
  }
}

enum SeparateMode
{
  ByNone,
  ByYMD,
  ByD,
}
enum FileNameFormat
{
  OriginalFileName,
  ShootingDateTimeNoSeparation,
  ShootingDateTimeSeparatedByUnderBar,
  ShootingDateTimeSeparatedByHyphen,
  CustomFormat,
}
enum DirectoryNameFormatByYMD
{
  YMD,
  CustomFormat,
}
enum DirectoryNameFormatByD
{
  YMDNoSeparation,
  YMDSeparatedByUnderBar,
  YMDSeparatedByHyphen,
  CustomFormat,
}
[Flags]
enum ImportOption
{
  Move = 2 ^ 0,
  SkipIfSame = 2 ^ 1,
  MakeExtensionLower = 2 ^ 2,
  MakeExtensionUpper = 2 ^ 3,
  AddCustomPictureExtension = 2 ^ 4,
  AddCustomMovieExtension = 2 ^ 5,
  ChangeWayToGetShootingDateTime = 2 ^ 6,
  UseASingleThread = 2 ^ 7,
}
[Flags]
enum FormatSpecifier
{
  Year = 2 ^ 0,
  Month = 2 ^ 1,
  Day = 2 ^ 2,
  Time = 2 ^ 3,
  FileName = 2 ^ 4,
}

static class SetCustomFormat
{
  public static string? SetCustomFileFormat()
  {
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the file name:");
      var usingSpecifier = FormatSpecifier.Year | FormatSpecifier.Month | FormatSpecifier.Day | FormatSpecifier.Time | FormatSpecifier.FileName;
      if (i == 0)
        OutputCustomFormatDescription(usingSpecifier);
      var customFileFormat = Console.ReadLine();
      if (customFileFormat != null && IsValidFormat(customFileFormat))
        return customFileFormat;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    return null;
  }
  public static string? SetCustomDirectoryFormatByDay()
  {
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the directory name:");
      var usingSpecifier = FormatSpecifier.Year | FormatSpecifier.Month | FormatSpecifier.Day;
      if (i == 0)
        OutputCustomFormatDescription(usingSpecifier);
      var customDirectoryFormatByDay = Console.ReadLine();
      if (customDirectoryFormatByDay != null && IsValidFormat(customDirectoryFormatByDay))
        return customDirectoryFormatByDay;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    return null;
  }
  public static (string year, string month, string day)? SetCustomDirectoryFormatByYearMonthAndDay()
  {
    string? year = "", month = "", day = "";
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the year directory name:");
      var usingSpecifier = FormatSpecifier.Year | FormatSpecifier.Month | FormatSpecifier.Day;
      if (i == 0)
        OutputCustomFormatDescription(usingSpecifier);
      year = Console.ReadLine();
      if (year != null && IsValidFormat(year))
        break;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the month directory name:");
      month = Console.ReadLine();
      if (month != null && IsValidFormat(month))
        break;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the day directory name:");
      day = Console.ReadLine();
      if (day != null && IsValidFormat(day))
        break;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    return (year ?? "", month ?? "", day ?? "");
  }

  private static void OutputCustomFormatDescription(FormatSpecifier usingSpecifier)
  {
    Console.WriteLine("You can use the following format specifiers:");
    if (usingSpecifier.HasFlag(FormatSpecifier.Year))
      Console.WriteLine("yyyy: Year like 2024");
    if (usingSpecifier.HasFlag(FormatSpecifier.Month))
      Console.WriteLine("MM: Month like 01");
    if (usingSpecifier.HasFlag(FormatSpecifier.Day))
      Console.WriteLine("dd: Day like 02");
    if (usingSpecifier.HasFlag(FormatSpecifier.Time))
    {
      Console.WriteLine("HH: Hour like 03 (24-hour clock)");
      Console.WriteLine("mm: Minute like 04");
      Console.WriteLine("ss: Second like 05");
    }
    if (usingSpecifier.HasFlag(FormatSpecifier.FileName))
      Console.WriteLine("NN: Original file name like IMG_0001 (This specifier is only available for this application)");
    Console.WriteLine("If you want to use different format specifiers, you can check other format specifiers on this page: https://learn.microsoft.com/dotnet/standard/base-types/custom-date-and-time-format-strings (Not all format specifiers are supported)");
    if (usingSpecifier.HasFlag(FormatSpecifier.Time))
      Console.WriteLine("For example, if you want to use the format yyyyMMdd_HHmmss, enter yyyyMMdd_HHmmss. It will be converted to 20240102_030405.");
    else
      Console.WriteLine("For example, if you want to use the format yyyyMMdd, enter yyyyMMdd. It will be converted to 20240102.");
    Console.WriteLine("You cannot use the following characters: /, \\, :, *, ?, \", <, >, |");
  }
  private static void OutputTryAgainMessage() => Console.Error.WriteLine("You have tried too many times. Try again from the beginning.");

  private static bool IsValidFormat(string format)
  {
    var testDate = DateTimeOffset.Now;
    try
    {
      var stringed = testDate.ToString(format);
      if (stringed.Contains('/') || stringed.Contains('\\') || stringed.Contains(':') || stringed.Contains('*') || stringed.Contains('?') || stringed.Contains('"') || stringed.Contains('<') || stringed.Contains('>') || stringed.Contains('|'))
        return false;
      return true;
    }
    catch
    {
      return false;
    }
  }
}

static partial class CustomFormatterExtension
{
  private static (int start, int end)[] CountQuots(string format)
  {
    List<(int start, int end)> result = [];
    char? target = null;
    int targetStart = -1;
    for (int i = 0; i < format.Length; i++)
    {
      if (target == '\"')
      {
        if (format[i] == '\"')
        {
          result.Add((targetStart, i));
          target = null;
        }
      }
      else if (target == '\'')
      {
        if (format[i] == '\'')
        {
          result.Add((targetStart, i));
          target = null;
        }
      }
      else
      {
        if (format[i] == '\"')
        {
          target = '\"';
          targetStart = i;
        }
        else if (format[i] == '\'')
        {
          target = '\'';
          targetStart = i;
        }
      }
    }
    return [.. result];
  }
  private static string ReplaceSpecifiers(string format, Func<Regex> formatSpecifier, string replacement)
  {
    var matches = formatSpecifier().Matches(format).Select(match => (match.Index, match.Length)).ToArray();
    foreach (var (start, length) in matches)
    {
      if (CountQuots(format).Any(range => range.start <= start && start <= range.end))
        continue;
      format = format.Remove(start, length).Insert(start, replacement);
    }
    return format;
  }
  private static string Unescape(string format)
  {
    return EscapeCharacter().Replace(format, "$1");
  }
  [GeneratedRegex(@"\\(.)")]
  private static partial Regex EscapeCharacter();

  public static string ToCustomYear(this int year, string format)
  {
    format = ReplaceSpecifiers(format, yFormatSpecifier, int.Parse(year.ToString("00000")[^2..]).ToString());

    format = ReplaceSpecifiers(format, yyFormatSpecifier, year.ToString("00000")[^2..]);

    format = ReplaceSpecifiers(format, yyyFormatSpecifier, year.ToString("00000")[^3..]);

    format = ReplaceSpecifiers(format, yyyyFormatSpecifier, year.ToString("00000")[^4..]);

    format = ReplaceSpecifiers(format, yyyyyFormatSpecifier, year.ToString("00000")[^5..]);

    return Unescape(format);
  }

  [GeneratedRegex("""(?<![y\\])y(?!y)""")]
  private static partial Regex yFormatSpecifier();

  [GeneratedRegex("""(?<![y\\])yy(?!y)""")]
  private static partial Regex yyFormatSpecifier();

  [GeneratedRegex("""(?<![y\\])yyy(?!y)""")]
  private static partial Regex yyyFormatSpecifier();

  [GeneratedRegex("""(?<![y\\])yyyy(?!y)""")]
  private static partial Regex yyyyFormatSpecifier();

  [GeneratedRegex("""(?<![y\\])yyyyy(?!y)""")]
  private static partial Regex yyyyyFormatSpecifier();


  public static string ToCustomMonth(this int month, string format)
  {
    format = ReplaceSpecifiers(format, MFormatSpecifier, int.Parse(month.ToString("00000")[^2..]).ToString());

    format = ReplaceSpecifiers(format, MMFormatSpecifier, month.ToString("00000")[^2..]);

    format = ReplaceSpecifiers(format, MMMFormatSpecifier, new DateOnly(1, month, 1).ToString("MMM"));

    format = ReplaceSpecifiers(format, MMMMFormatSpecifier, new DateOnly(1, month, 1).ToString("MMMM"));

    return Unescape(format);
  }

  [GeneratedRegex("""(?<![M\\])M(?!M)""")]
  private static partial Regex MFormatSpecifier();

  [GeneratedRegex("""(?<![M\\])MM(?!M)""")]
  private static partial Regex MMFormatSpecifier();

  [GeneratedRegex("""(?<![M\\])MMM(?!M)""")]
  private static partial Regex MMMFormatSpecifier();

  [GeneratedRegex("""(?<![M\\])MMMM(?!M)""")]
  private static partial Regex MMMMFormatSpecifier();


  public static string ToCustomDay(this int day, string format)
  {
    format = ReplaceSpecifiers(format, dFormatSpecifier, int.Parse(day.ToString("00000")[^2..]).ToString());

    format = ReplaceSpecifiers(format, ddFormatSpecifier, day.ToString("00000")[^2..]);

    format = ReplaceSpecifiers(format, dddFormatSpecifier, new DateOnly(1, 1, day).ToString("ddd"));

    format = ReplaceSpecifiers(format, ddddFormatSpecifier, new DateOnly(1, 1, day).ToString("dddd"));

    return Unescape(format);
  }

  [GeneratedRegex("""(?<![d\\])d(?!d)""")]
  private static partial Regex dFormatSpecifier();

  [GeneratedRegex("""(?<![d\\])dd(?!d)""")]
  private static partial Regex ddFormatSpecifier();

  [GeneratedRegex("""(?<![d\\])ddd(?!d)""")]
  private static partial Regex dddFormatSpecifier();

  [GeneratedRegex("""(?<![d\\])dddd(?!d)""")]
  private static partial Regex ddddFormatSpecifier();


  public static string ToCustomFileName(this string format, string fileName)
  {
    return ReplaceSpecifiers(format, fileNameFormatSpecifier, fileName);
  }

  [GeneratedRegex("""(?<![N\\])NN(?!N)""")]
  private static partial Regex fileNameFormatSpecifier();
}

class Progress(int width, int total)
{
  public int columns = Console.WindowWidth;
  public int width = width;
  public int current = 0;
  public int total = total;
  private int rowLate = Console.CursorTop;

  public void Update(string message)
  {
    int row0 = Console.CursorTop;

    float parcent = (float)current / total;
    int widthNow = (int)Math.Floor(width * parcent);

    string gauge = new string('>', widthNow) + new string(' ', width - widthNow);
    string status = $"({parcent * 100:f1}%<-{current}/{total})";

    Console.WriteLine($"#[{gauge}]#{status}");
    ClearScreenDown();

    Console.WriteLine(message);
    rowLate = Console.CursorTop;
    Console.SetCursorPosition(0, row0);
    current++;
  }

  public void Done(string doneAlert)
  {
    int sideLen = (int)Math.Floor((float)(width - doneAlert.Length) / 2);

    string gauge = new string('=', sideLen) + doneAlert;
    gauge += new string('=', width - gauge.Length);
    string status = $"(100%<-{total}/{total})";

    Console.WriteLine($"#[{gauge}]#{status}");
  }

  private void ClearScreenDown()
  {
    int clearRange = rowLate - (Console.CursorTop - 1);
    Console.Write(new string(' ', columns * clearRange));
    Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - clearRange);
  }
}
