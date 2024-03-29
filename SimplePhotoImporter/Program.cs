using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable CA1416 // プラットフォームの互換性を検証

START:

string[] pictureExtensions = [".jpeg", ".jpg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".ico", ".svg", ".heic"];
string[] movieExtensions = [".mov", ".mp4", ".m2ts", ".mts", ".avi", ".flv"];

var version = typeof(Program).Assembly.GetName().Version;
Console.WriteLine("Simple Photo Importer v" + version);

if (!OperatingSystem.IsWindows() || Environment.OSVersion.Version.Major < 10)
{
  Console.Error.WriteLine("Not supported OS.");
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

GroupingMode groupingMode;
while (true)
{
  Console.WriteLine("Enter the mode to for grouping files by directory:");
  Console.WriteLine("1: By none");
  Console.WriteLine("2: By year, month, and day");
  Console.WriteLine("3: By day");
  var inputGroupMode = Console.ReadLine();

  if (inputGroupMode == "1")
  {
    groupingMode = GroupingMode.ByNone;
    break;
  }
  else if (inputGroupMode == "2")
  {
    groupingMode = GroupingMode.ByYMD;
    break;
  }
  else if (inputGroupMode == "3")
  {
    groupingMode = GroupingMode.ByD;
    break;
  }
  else if (inputGroupMode == null)
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
      formatRule.file = FileNameFormat.ShootingDateTimeNoGrouping;
      break;
    case "3":
      formatRule.file = FileNameFormat.ShootingDateTimeGroupedByUnderBar;
      break;
    case "4":
      formatRule.file = FileNameFormat.ShootingDateTimeGroupedByHyphen;
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
if (groupingMode == GroupingMode.ByYMD)
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
if (groupingMode == GroupingMode.ByD)
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
        formatRule.directoryByD = DirectoryNameFormatByD.YMDNoGrouping;
        break;
      case "2":
        formatRule.directoryByD = DirectoryNameFormatByD.YMDGroupedByUnderBar;
        break;
      case "3":
        formatRule.directoryByD = DirectoryNameFormatByD.YMDGroupedByHyphen;
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

ConflictResolution conflictResolution = ConflictResolution.AddNumber;
while (true)
{
  Console.WriteLine("Enter the conflict resolution:");
  Console.WriteLine("1: Skip if the contents are the same, otherwise, add a number to the file name. (Recommended)");
  Console.WriteLine("2: Add a number to the file name");
  Console.WriteLine("3: Make a new directory having name with a number and add files to the directory");
  Console.WriteLine("4: Skip");
  Console.WriteLine("5: Skip by directory (All files which will be copied to the same directory will be skipped)");
  var inputConflictResolution = Console.ReadLine();
  if (string.IsNullOrWhiteSpace(inputConflictResolution))
    continue;
  switch (inputConflictResolution)
  {
    case "1":
      conflictResolution = ConflictResolution.SkipIfSame;
      break;
    case "2":
      conflictResolution = ConflictResolution.AddNumber;
      break;
    case "3":
      conflictResolution = ConflictResolution.AddNumberToDirectory;
      break;
    case "4":
      conflictResolution = ConflictResolution.Skip;
      break;
    case "5":
      conflictResolution = ConflictResolution.SkipByDirectory;
      break;
    default:
      Console.Error.WriteLine("Invalid input.");
      continue;
  }
  break;
}

ImportOption option = 0;
while (true)
{
  Console.WriteLine("Enter the options you want to enable. You can enter multiple options by separating them with a space:");
  Console.WriteLine("1: Move instead of copy");
  Console.WriteLine("2: If the same name file already exists, check if the files are the same and skip if they are the same, otherwise, add a number to the file name. If this option is not enabled, always add a number to the file name");
  Console.WriteLine("3: Make the extension lower case");
  Console.WriteLine("4: Make the extension upper case");
  Console.WriteLine("5: Add custom picture extension");
  Console.WriteLine("6: Add custom movie extension");
  Console.WriteLine("7: Change the way to get shooting date time");
  Console.WriteLine("8: Use a single thread");
  Console.WriteLine("Enter nothing to finish");
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
      option |= ImportOption.MakeExtensionLower;
    else if (opt == "3")
      option |= ImportOption.MakeExtensionUpper;
    else if (opt == "4")
      option |= ImportOption.AddCustomPictureExtension;
    else if (opt == "5")
      option |= ImportOption.AddCustomMovieExtension;
    else if (opt == "6")
      option |= ImportOption.ChangeWayToGetShootingDateTime;
    else if (opt == "7")
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

WayToGetShootingDateTime[] wayToGetShootingDateTime = [WayToGetShootingDateTime.Exif, WayToGetShootingDateTime.MediaCreated, WayToGetShootingDateTime.Creation, WayToGetShootingDateTime.Modified, WayToGetShootingDateTime.Access];
if (option.HasFlag(ImportOption.ChangeWayToGetShootingDateTime))
{
  while (true)
  {
    Console.WriteLine("Sort the following in order of priority to get the shooting date time and enter the numbers separated by a space:");
    Console.WriteLine("1: Exif (Only for pictures)");
    Console.WriteLine("2: Media created (Only for movies. Media created doesn't include seconds, so if Media created and Creation or Modified are close, use Creation or Modified, otherwise use 0");
    Console.WriteLine("3: Creation");
    Console.WriteLine("4: Modified");
    Console.WriteLine("5: Access");
    Console.WriteLine("Default: Exif -> Media created -> Creation -> Modified -> Access");
    var inputWayToGetShootingDateTime = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(inputWayToGetShootingDateTime))
      continue;
    var ways = inputWayToGetShootingDateTime.Split(' ');
    if (ways.Length != 5 || MatchNotWayToGetShootingDateTime().IsMatch(inputWayToGetShootingDateTime) || ways.Distinct().Count() != 5)
    {
      Console.Error.WriteLine("You must specify all options.");
      continue;
    }
    for (int i = 0; i < ways.Length; i++)
    {
      wayToGetShootingDateTime[i] = ways[i] switch
      {
        "1" => WayToGetShootingDateTime.Exif,
        "2" => WayToGetShootingDateTime.MediaCreated,
        "3" => WayToGetShootingDateTime.Creation,
        "4" => WayToGetShootingDateTime.Modified,
        "5" => WayToGetShootingDateTime.Access,
        _ => throw new InvalidOperationException(),
      };
    }
    break;
  }
}

Type? shellAppType = Type.GetTypeFromProgID("Shell.Application");
if (shellAppType == null)
{
  Console.Error.WriteLine("Failed to get the type of Shell.Application.");
  return;
}
dynamic? shell = Activator.CreateInstance(shellAppType);
if (shell == null)
{
  Console.Error.WriteLine("Failed to create an instance of Shell.Application.");
  return;
}
string? objFolderPath = null;
Shell32.Folder? objFolder = null;

var threadCount = option.HasFlag(ImportOption.UseASingleThread) ? 1 : Environment.ProcessorCount;
var startTime = DateTimeOffset.Now;

if (groupingMode == GroupingMode.ByNone)
{
  var files = GetFiles(sourcePath);

  var progress = new Progress(50, files.Count);
  files.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(file =>
  {
    var fileName = Path.GetFileName(file.Key);
    var newFilePath = GenerateNewFilePath(destPath, fileName, file.Value);
    CopyFile(file.Key, newFilePath, file.Value, option);
    lock (progress)
      progress.Update("");
  });
  progress.Done("Done");
}
else if (groupingMode == GroupingMode.ByYMD)
{
  var files = GetFilesGroupedByYearAndMonthAndDay(sourcePath);

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
else if (groupingMode == GroupingMode.ByD)
{
  var files = GetFilesGroupedByDay(sourcePath);

  var progress = new Progress(50, files.filesCount);
  files.files.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(day =>
  {
    Directory.CreateDirectory(day.Key);

    day.Value.AsParallel().WithDegreeOfParallelism(threadCount).ForAll(file =>
    {
      CopyFile(file.sourceFilePath, file.destFilePath, file.date, option);
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
    DisplayFailed();
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
    DisplayFailed();
  }
  catch (Exception e)
  {
    Console.Error.WriteLine($"Failed to copy {sourceFileName} to {destFileName}: {e.Message}");
  }

  void DisplayFailed()
  {
    Console.Error.WriteLine($"Failed to copy {sourceFileName} to {destFileName}: The file already exists.");
  }
}

(int filesCount, Dictionary<int, Dictionary<int, Dictionary<int, (string path, DateTimeOffset date)[]>>> files) GetFilesGroupedByYearAndMonthAndDay(string parentDirectory)
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
(int filesCount, Dictionary<string, (string sourceFilePath, string destFilePath, DateTimeOffset date)[]> files) GetFilesGroupedByDay(string parentDirectory)
{
  var files = GetFiles(parentDirectory);

  List<string> skippedDirectories = [];
  List<(string original, string changed)> addedNumberDirectories = [];
  var separatedFiles = files
   .GroupBy(file => new DateOnly(file.Value.Year, file.Value.Month, file.Value.Day))
   .Select(dayGroup =>
   {
     string? directoryPath;
     if (formatRule.directoryByD == DirectoryNameFormatByD.CustomFormat && customDirectoryFormatByD != null)
       directoryPath = Path.Combine(destPath, CustomFormatterExtension.ToCustomYear(dayGroup.Key.Year, customDirectoryFormatByD), CustomFormatterExtension.ToCustomMonth(dayGroup.Key.Month, customDirectoryFormatByD), CustomFormatterExtension.ToCustomDay(dayGroup.Key.Day, customDirectoryFormatByD));
     else
       directoryPath = Path.Combine(destPath, dayGroup.Key.ToString(formatRule.directoryByD switch
       {
         DirectoryNameFormatByD.YMDNoGrouping => "yyyyMMdd",
         DirectoryNameFormatByD.YMDGroupedByUnderBar => "yyyy_MM_dd",
         DirectoryNameFormatByD.YMDGroupedByHyphen => "yyyy-MM-dd",
         _ => throw new InvalidOperationException(),
       }));
     return (directoryPath, files: dayGroup.ToArray());
   })
   .ToDictionary(dayGroup => dayGroup.directoryPath,
   dayGroup =>
   {
     var files = dayGroup.files;
     Array.Sort(files, (x, y) => x.Value.CompareTo(y.Value));
     return files
     .GroupBy(files => GenerateNewFilePath(dayGroup.directoryPath, Path.GetFileName(files.Key), files.Value))
     .Select(x =>
     {
       var newFilePath = x.Key;
       var files = x.ToArray();

       if (skippedDirectories.Contains(dayGroup.directoryPath))
         return [];
       if (addedNumberDirectories.Any(x => x.original == dayGroup.directoryPath))
       {
         var changed = addedNumberDirectories.First(x => x.original == dayGroup.directoryPath).changed;
         newFilePath = Path.Combine(changed, Path.GetFileName(newFilePath));
       }

       if (File.Exists(newFilePath))
       {
         var i = 2;
         if (conflictResolution == ConflictResolution.SkipIfSame)
         {
           return files.Where(file => (!IsSameFile(file.Key, newFilePath)))
           .Select(file =>
           {
             while (true)
             {
               var renewedFilePath = Path.Combine(Path.GetDirectoryName(newFilePath) ?? "", $"{Path.GetFileNameWithoutExtension(newFilePath)} ({i++}){Path.GetExtension(newFilePath)}");
               if (File.Exists(renewedFilePath))
                 continue;
               else
                 return (file.Key, renewedFilePath, file.Value);
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
                 return (file.Key, renewedFilePath, file.Value);
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
             var fileName = Path.GetFileName(file.Key);
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
           return files.Zip(filePaths, (file, filePath) => (file.Key, filePath, file.Value));
         }
         else if (conflictResolution == ConflictResolution.SkipByDirectory)
         {
           skippedDirectories.Add(dayGroup.directoryPath);
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
           1 => files.Select(file => (file.Key, newFilePath, file.Value)),
           _ => AddNumber(),
         };
       }

       IEnumerable<(string, string, DateTimeOffset)> AddNumber()
       {
         var i = 1;
         return files.Select(file =>
         {
           if (i++ == 1) return (file.Key, newFilePath, file.Value);

           while (true)
           {
             var renewedFilePath = Path.Combine(Path.GetDirectoryName(newFilePath) ?? "", $"{Path.GetFileNameWithoutExtension(newFilePath)} ({i++}){Path.GetExtension(newFilePath)}");
             if (File.Exists(renewedFilePath))
               continue;
             else
               return (file.Key, renewedFilePath, file.Value);
           }
         });
       }
     }).SelectMany(x => x).ToArray();
   });

  if (skippedDirectories.Count > 0)
  {
    for (var i = 0; i < skippedDirectories.Count; i++)
    {
      separatedFiles[skippedDirectories[i]] = [];
    }
  }
  if (addedNumberDirectories.Count > 0)
  {
    for (var i = 0; i < addedNumberDirectories.Count; i++)
    {
      var original = addedNumberDirectories[i].original;
      var changed = addedNumberDirectories[i].changed;
      var containedFiles = separatedFiles[original];
      separatedFiles.Remove(original);
      for (var j = containedFiles.Length - 1; j >= 0; j--)
      {
        if (containedFiles[j].Item2.StartsWith(original + "\\"))
          containedFiles[j].Item2 = containedFiles[j].Item2[original.Length..].Insert(0, changed);
      }
      separatedFiles[changed] = containedFiles;
    }
  }

  return (separatedFiles.SelectMany(x => x.Value).Count(), separatedFiles);
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
  var isMovie = movieExtensions.Contains(Path.GetExtension(filePath).ToLower());
  var ways = isMovie ? wayToGetShootingDateTime.Where(way => way != WayToGetShootingDateTime.Exif) : wayToGetShootingDateTime.Where(way => way != WayToGetShootingDateTime.MediaCreated);
  DateTimeOffset? creation = null;
  DateTimeOffset? modified = null;
  foreach (var way in ways)
  {
    DateTimeOffset? date;
    switch (way)
    {
      case WayToGetShootingDateTime.Exif:
        date = ByExif(filePath);
        break;
      case WayToGetShootingDateTime.MediaCreated:
        date = ByMediaCreated(filePath, ref creation, ref modified);
        break;
      case WayToGetShootingDateTime.Creation:
        if (creation != null)
          return creation.Value;
        date = creation = ByCreation(filePath);
        break;
      case WayToGetShootingDateTime.Modified:
        if (modified != null)
          return modified.Value;
        date = modified = ByModified(filePath);
        break;
      case WayToGetShootingDateTime.Access:
        date = ByAccess(filePath);
        break;
      default:
        throw new InvalidOperationException();
    }
    if (date != null)
      return date.Value;
  }
  return new DateTimeOffset(File.GetCreationTime(filePath));

  DateTimeOffset? ByExif(string filePath)
  {
    try
    {
      using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
      using var image = Image.FromStream(fileStream, false, false);
      var item = image.GetPropertyItem(36867);
      if (item == null || item.Value == null || item.Value.Length == 0)
        return null;
      var picDateStr = Encoding.UTF8.GetString(item.Value).Trim(['\0']);
      return DateTimeOffset.ParseExact(picDateStr, "yyyy:MM:dd HH:mm:ss", null);
    }
    catch
    {
      return null;
    }
  }
  DateTimeOffset? ByMediaCreated(string filePath, ref DateTimeOffset? creation, ref DateTimeOffset? modified)
  {
    try
    {
      var directoryPath = Path.GetDirectoryName(filePath);
      if (objFolder == null || objFolderPath != directoryPath)
      {
        objFolder = shell.NameSpace(directoryPath);
        objFolderPath = directoryPath;
      }
      Shell32.FolderItem folderItem = objFolder.ParseName(Path.GetFileName(filePath));
      var dateString = objFolder.GetDetailsOf(folderItem, 208);
      if (dateString == null)
        return null;
      dateString = MatchOtherThanNumbers().Replace(dateString, "");
      if (dateString.Length != 12)
        return null;
      var mediaCreated = DateTimeOffset.ParseExact(dateString, "yyyyMMddHHmm", null);

      creation ??= ByCreation(filePath);
      modified ??= ByModified(filePath);

      var precedence = wayToGetShootingDateTime.First(x => x == WayToGetShootingDateTime.Creation || x == WayToGetShootingDateTime.Modified);
      var first = precedence == WayToGetShootingDateTime.Creation ? creation : modified;
      var second = precedence == WayToGetShootingDateTime.Creation ? modified : creation;
      if (first != null && second != null && Math.Abs((mediaCreated - first.Value).TotalMinutes) < 2)
        return first.Value;
      else
      {
        if (second != null && Math.Abs((mediaCreated - second.Value).TotalMinutes) < 2)
          return second.Value;
        else
          return mediaCreated;
      }
    }
    catch
    {
      return null;
    }
  }
  DateTimeOffset? ByCreation(string filePath)
  {
    try
    {
      return new DateTimeOffset(File.GetCreationTime(filePath));
    }
    catch
    {
      return null;
    }
  }
  DateTimeOffset? ByModified(string filePath)
  {
    try
    {
      return new DateTimeOffset(File.GetLastWriteTime(filePath));
    }
    catch
    {
      return null;
    }
  }
  DateTimeOffset? ByAccess(string filePath)
  {
    try
    {
      return new DateTimeOffset(File.GetLastAccessTime(filePath));
    }
    catch
    {
      return null;
    }
  }
}
[MethodImpl(MethodImplOptions.AggressiveInlining)]
string GenerateNewFilePath(string directoryPath, string originalFileName, DateTimeOffset shootingDate)
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
      FileNameFormat.ShootingDateTimeNoGrouping => Path.Combine(directoryPath, $"{shootingDate:yyyyMMddHHmmss}{extension}"),
      FileNameFormat.ShootingDateTimeGroupedByUnderBar => Path.Combine(directoryPath, $"{shootingDate:yyyy_MM_dd_HH_mm_ss}{extension}"),
      FileNameFormat.ShootingDateTimeGroupedByHyphen => Path.Combine(directoryPath, $"{shootingDate:yyyy-MM-dd-HH-mm-ss}{extension}"),
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

enum GroupingMode
{
  ByNone,
  ByYMD,
  ByD,
}
enum FileNameFormat
{
  OriginalFileName,
  ShootingDateTimeNoGrouping,
  ShootingDateTimeGroupedByUnderBar,
  ShootingDateTimeGroupedByHyphen,
  CustomFormat,
}
enum DirectoryNameFormatByYMD
{
  YMD,
  CustomFormat,
}
enum DirectoryNameFormatByD
{
  YMDNoGrouping,
  YMDGroupedByUnderBar,
  YMDGroupedByHyphen,
  CustomFormat,
}
enum ConflictResolution
{
  SkipIfSame,
  AddNumber,
  AddNumberToDirectory,
  Skip,
  SkipByDirectory,
}
[Flags]
enum ImportOption
{
  Move = 1,
  MakeExtensionLower = 2,
  MakeExtensionUpper = 4,
  AddCustomPictureExtension = 8,
  AddCustomMovieExtension = 16,
  ChangeWayToGetShootingDateTime = 32,
  UseASingleThread = 128,
}
[Flags]
enum FormatSpecifier
{
  Year = 1,
  Month = 2,
  Day = 4,
  Time = 8,
  FileName = 16,
}
enum WayToGetShootingDateTime
{
  Exif,
  MediaCreated,
  Creation,
  Modified,
  Access,
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
      Console.WriteLine("NN: Original file name like IMG_0001 (Without extension. This specifier is only available for this application)");
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
    return ReplaceSpecifiers(format, fileNameFormatSpecifier, Path.GetFileNameWithoutExtension(fileName));
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

partial class Program
{
  [GeneratedRegex("""[^1-5 ]""")]
  private static partial Regex MatchNotWayToGetShootingDateTime();

  [GeneratedRegex(@"[^0-9]")]
  private static partial Regex MatchOtherThanNumbers();
}
