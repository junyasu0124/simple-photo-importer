using SimplePhotoImporter.Consoles;
using System.Globalization;
using System.Text;
using static SimplePhotoImporter.Checks.Checks;
using static SimplePhotoImporter.CustomRegexes;
using static SimplePhotoImporter.Files.Files;
using static SimplePhotoImporter.Files.GetFiles.GetFiles;
using static SimplePhotoImporter.Files.Utils.FileUtils;
using static SimplePhotoImporter.Usage.Usage;

namespace SimplePhotoImporter;

public partial class Program
{
  private static void Main(string[] args)
  {
    if (!OperatingSystem.IsWindows() || Environment.OSVersion.Version.Major < 10)
    {
      Console.Error.WriteLine("Not supported OS.");
      return;
    }

    var version = typeof(Program).Assembly.GetName().Version;
    Print("Simple Photo Importer v" + version);

    InitializeConsole();

    HashSet<string> photoExtensions = [".jpeg", ".jpg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".ico", ".svg", ".heic"];
    HashSet<string> videoExtensions = [".mov", ".mp4", ".m2ts", ".mts", ".avi", ".flv"];

    WayToGetShootingDateTime[] wayToGetShootingDateTime = [
      WayToGetShootingDateTime.Exif, WayToGetShootingDateTime.MediaCreated, WayToGetShootingDateTime.Creation, WayToGetShootingDateTime.Modified, WayToGetShootingDateTime.Access
    ];

    var (canContinue, needInteractive, data) = Read(args, photoExtensions, videoExtensions, wayToGetShootingDateTime);

    if (!canContinue)
      return;


    string[] sourcePaths = [];
    string[] excludedSourcePaths = [];

    string[] destPaths = [];

    GroupingMode groupingMode = GroupingMode.NoGrouping;

    (FileNameFormat File, DirectoryNameFormatByYMD DirectoryByYMD, DirectoryNameFormatByYD DirectoryByYD, DirectoryNameFormatByD DirectoryByD) nameFormat = (0, 0, 0, 0);
    string? customFileFormat = null;
    (string Year, string Month, string Day)? customDirectoryFormatByYMD = null;
    (string Year, string Day)? customDirectoryFormatByYD = null;
    string? customDirectoryFormatByD = null;

    ConflictResolution conflictResolution = ConflictResolution.AddNumber;

    ImportOption option = 0;
    List<(DateTimeOffset Start, DateTimeOffset End)>? dateRanges = null;

    if (data != null && !needInteractive)
    {
      (sourcePaths, excludedSourcePaths, destPaths, groupingMode, nameFormat, customFileFormat, customDirectoryFormatByYMD, customDirectoryFormatByYD, customDirectoryFormatByD, conflictResolution, option, dateRanges, photoExtensions, videoExtensions, wayToGetShootingDateTime) = data.Value;
    }

    if (needInteractive)
    {
      Print("Enter the paths to the folder you want to import FROM. You can enter multiple paths by separating them with a space (use double quotes if the path contains a space).");
      Print("If you want to exclude a directory, enter the path with a asterisk at the beginning.");
      Print("e.g. \"C:\\Users\\user\\Pictures Folder\" *\"C:\\Users\\user\\Pictures Folder\\excluded\"");
      while (true)
      {
        var inputFromPath = Console.ReadLine();

        var (message, paths, excludedPaths) = CheckInputDirectories(inputFromPath, true);
        if (message != null)
        {
          PrintError(message);
          continue;
        }
        sourcePaths = paths ?? [];
        excludedSourcePaths = excludedPaths ?? [];
        break;
      }

      Print("Enter the paths to the folder you want to import TO. You can enter multiple paths by separating them with a space (use double quotes if the path contains a space).");
      while (true)
      {
        var inputToPath = Console.ReadLine();

        var (message, paths, _) = CheckInputDirectories(inputToPath, false);
        if (message != null)
        {
          PrintError(message);
          continue;
        }
        destPaths = paths ?? [];
        break;
      }

      Print("Enter the mode to for grouping files by directory:");
      Print("1: No grouping like \\IMG_0001.jpg");
      Print("2: By year, month, and day like \\2024\\01\\02\\IMG_0001.jpg");
      Print("3: By year and day like \\2024\\20240102\\IMG_0001.jpg");
      Print("4: By day like \\20240102\\IMG_0001.jpg");
      while (true)
      {
        var inputGroupMode = Console.ReadLine();

        if (inputGroupMode == "1")
        {
          groupingMode = GroupingMode.NoGrouping;
          break;
        }
        else if (inputGroupMode == "2")
        {
          groupingMode = GroupingMode.GroupedByYMD;
          break;
        }
        else if (inputGroupMode == "3")
        {
          groupingMode = GroupingMode.GroupedByYD;
          break;
        }
        else if (inputGroupMode == "4")
        {
          groupingMode = GroupingMode.GroupedByD;
          break;
        }
        else if (inputGroupMode == null)
        {
          continue;
        }
        else
        {
          PrintError("Invalid input.");
        }
      }

      Print("Enter the format of the file name:");
      Print("1: Original file name");
      Print("2: Shooting date time as yyyyMMddHHmmss");
      Print("3: Shooting date time as yyyy_MM_dd_HH_mm_ss");
      Print("4: Shooting date time as yyyy-MM-dd-HH-mm-ss");
      Print("5: Custom format");
      while (true)
      {
        var inputFileNameFormat = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(inputFileNameFormat))
          continue;
        switch (inputFileNameFormat)
        {
          case "1":
            nameFormat.File = FileNameFormat.OriginalFileName;
            break;
          case "2":
            nameFormat.File = FileNameFormat.ShootingDateTimeNoGrouping;
            break;
          case "3":
            nameFormat.File = FileNameFormat.ShootingDateTimeGroupedByUnderBar;
            break;
          case "4":
            nameFormat.File = FileNameFormat.ShootingDateTimeGroupedByHyphen;
            break;
          case "5":
            nameFormat.File = FileNameFormat.CustomNameFormat;
            customFileFormat = SetCustomFileFormat();
            if (customFileFormat == null)
              return;
            break;
          default:
            PrintError("Invalid input.");
            continue;
        }
        break;
      }
      switch (groupingMode)
      {
        case GroupingMode.GroupedByYMD:
          {
            Print("Enter the format of the directory name:");
            Print("1: Year\\Month\\Day as yyyy\\MM\\dd");
            Print("2: Custom format");
            while (true)
            {
              var inputDirectoryNameFormatByYMD = Console.ReadLine();
              if (string.IsNullOrWhiteSpace(inputDirectoryNameFormatByYMD))
                continue;
              switch (inputDirectoryNameFormatByYMD)
              {
                case "1":
                  nameFormat.DirectoryByYMD = DirectoryNameFormatByYMD.YMDNoSeparation;
                  break;
                case "2":
                  nameFormat.DirectoryByYMD = DirectoryNameFormatByYMD.CustomNameFormat;
                  customDirectoryFormatByYMD = SetCustomDirectoryFormatByYMD();
                  if (customDirectoryFormatByYMD == null)
                    return;
                  break;
                default:
                  PrintError("Invalid input.");
                  continue;
              }
              break;
            }
            break;
          }
        case GroupingMode.GroupedByYD:
          {
            Print("Enter the format of the directory name:");
            Print("1: Year\\YearMonthDay as yyyy\\yyyyMMdd");
            Print("2: Year\\Year_Month_Day as yyyy\\yyyy_MM_dd");
            Print("3: Year\\Year-Month-Day as yyyy\\yyyy-MM-dd");
            Print("4: Year\\MonthDay as yyyy\\MMdd");
            Print("5: Year\\Month_Day as yyyy\\MM_dd");
            Print("6: Year\\Month-Day as yyyy\\MM-dd");
            Print("7: Custom format");
            while (true)
            {
              var inputDirectoryNameFormatByYD = Console.ReadLine();
              if (string.IsNullOrWhiteSpace(inputDirectoryNameFormatByYD))
                continue;
              switch (inputDirectoryNameFormatByYD)
              {
                case "1":
                  nameFormat.DirectoryByYD = DirectoryNameFormatByYD.YYMDNoSeparation;
                  break;
                case "2":
                  nameFormat.DirectoryByYD = DirectoryNameFormatByYD.YYMDSeparatedByUnderBar;
                  break;
                case "3":
                  nameFormat.DirectoryByYD = DirectoryNameFormatByYD.YYMDSeparatedByHyphen;
                  break;
                case "4":
                  nameFormat.DirectoryByYD = DirectoryNameFormatByYD.YMDNoSeparation;
                  break;
                case "5":
                  nameFormat.DirectoryByYD = DirectoryNameFormatByYD.YMDSeparatedByUnderBar;
                  break;
                case "6":
                  nameFormat.DirectoryByYD = DirectoryNameFormatByYD.YMDSeparatedByHyphen;
                  break;
                case "7":
                  nameFormat.DirectoryByYD = DirectoryNameFormatByYD.CustomNameFormat;
                  customDirectoryFormatByYD = SetCustomDirectoryFormatByYD();
                  if (customDirectoryFormatByYD == null)
                    return;
                  break;
                default:
                  PrintError("Invalid input.");
                  continue;
              }
              break;
            }
            break;
          }
        case GroupingMode.GroupedByD:
          {
            Print("Enter the format of the directory name:");
            Print("1: YearMonthDay as yyyyMMdd");
            Print("2: Year_Month_Day as yyyy_MM_dd");
            Print("3: Year-Month-Day as yyyy-MM-dd");
            Print("4: Custom format");
            while (true)
            {
              var inputDirectoryNameFormatByD = Console.ReadLine();
              if (string.IsNullOrWhiteSpace(inputDirectoryNameFormatByD))
                continue;
              switch (inputDirectoryNameFormatByD)
              {
                case "1":
                  nameFormat.DirectoryByD = DirectoryNameFormatByD.YMDNoSeparation;
                  break;
                case "2":
                  nameFormat.DirectoryByD = DirectoryNameFormatByD.YMDSeparatedByUnderBar;
                  break;
                case "3":
                  nameFormat.DirectoryByD = DirectoryNameFormatByD.YMDSeparatedByHyphen;
                  break;
                case "4":
                  nameFormat.DirectoryByD = DirectoryNameFormatByD.CustomNameFormat;
                  customDirectoryFormatByD = SetCustomDirectoryFormatByDay();
                  if (customDirectoryFormatByD == null)
                    return;
                  break;
                default:
                  PrintError("Invalid input.");
                  continue;
              }
              break;
            }
            break;
          }
      }

      var isEnabledAddNumberToDirectory = groupingMode != GroupingMode.NoGrouping;
      Print("Enter the conflict resolution. (It will be applied only to files that existed before importing. If multiple files with the same name are created in the same directory, a number will be added to the file name):");
      Print("1: Skip if the contents are the same, otherwise, add a number to the file name. (Recommended)");
      Print("2: Skip");
      Print("3: Skip by directory (All files which will be copied to the same directory will be skipped)");
      Print("4: Add a number to the file name");
      while (true)
      {
        if (isEnabledAddNumberToDirectory)
        {
          var addNumberDescription = "";
          if (groupingMode == GroupingMode.GroupedByYMD)
          {
            if (nameFormat.DirectoryByYMD == DirectoryNameFormatByYMD.CustomNameFormat && customDirectoryFormatByYMD != null)
              addNumberDescription = $". e.g. {customDirectoryFormatByYMD.Value.Year}\\{customDirectoryFormatByYMD.Value.Month}\\{customDirectoryFormatByYMD.Value.Day} (2)";
            addNumberDescription = ". e.g. yyyy\\MM\\dd (2)";
          }
          else if (groupingMode == GroupingMode.GroupedByYD)
          {
            if (nameFormat.DirectoryByYD == DirectoryNameFormatByYD.CustomNameFormat && customDirectoryFormatByYD != null)
              addNumberDescription = $". e.g. {customDirectoryFormatByYD.Value.Year}\\{customDirectoryFormatByYD.Value.Day} (2)";
            addNumberDescription = ". e.g. yyyy\\yyyyMMdd (2)";
          }
          Print("5: Make a new directory having name with a number and add files to the directory" + addNumberDescription);
        }
        var inputConflictResolution = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(inputConflictResolution))
          continue;
        switch (inputConflictResolution)
        {
          case "1":
            conflictResolution = ConflictResolution.SkipIfSame;
            break;
          case "2":
            conflictResolution = ConflictResolution.Skip;
            break;
          case "3":
            conflictResolution = ConflictResolution.SkipByDirectory;
            break;
          case "4":
            conflictResolution = ConflictResolution.AddNumber;
            break;
          case "5":
            if (isEnabledAddNumberToDirectory)
              conflictResolution = ConflictResolution.AddNumberToDirectory;
            else
            {
              PrintError("Invalid input.");
              continue;
            }
            break;
          default:
            PrintError("Invalid input.");
            continue;
        }
        break;
      }

      Print("Enter the options you want to enable. You can enter multiple options by separating them with a space:");
      Print("1: Move instead of copy");
      Print("2: Specify a range of files to import by date");
      Print("3: Make the extension lower case");
      Print("4: Make the extension upper case");
      Print("5: Add custom photo extension");
      Print("6: Add custom video extension");
      Print("7: Change priority of the way to get shooting date time");
      Print("8: Use a single thread");
      Print("9: Output log");
      Print("Enter nothing to finish");
      while (true)
      {
        var inputOption = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(inputOption))
          break;
        var options = inputOption.Split(' ');
        option = 0;
        var invalid = false;
        if (options.Length != options.Distinct().Count())
        {
          PrintError("You can't specify the same option multiple times.");
        }
        foreach (var opt in options)
        {
          if (opt == "1")
            option |= ImportOption.Move;
          else if (opt == "2")
            option |= ImportOption.Range;
          else if (opt == "3")
            option |= ImportOption.MakeExtensionLower;
          else if (opt == "4")
            option |= ImportOption.MakeExtensionUpper;
          else if (opt == "5")
            option |= ImportOption.AddCustomPhotoExtension;
          else if (opt == "6")
            option |= ImportOption.AddCustomVideoExtension;
          else if (opt == "7")
            option |= ImportOption.ChangeDateInfoPriority;
          else if (opt == "8")
            option |= ImportOption.UseASingleThread;
          else if (opt == "9")
            option |= ImportOption.Log;
          else
          {
            PrintError("Invalid input.");
            invalid = true;
            break;
          }
        }
        if (invalid)
          continue;
        if (option.HasFlag(ImportOption.MakeExtensionLower) && option.HasFlag(ImportOption.MakeExtensionUpper))
        {
          PrintError("You can't specify both 3 and 4.");
          continue;
        }
        break;
      }

      if (option.HasFlag(ImportOption.Range))
      {
        var dateFormat = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
        var timeFormat = DateTimeFormatInfo.CurrentInfo.LongTimePattern;
        Print($"""Enter the range of files to import by date. The format is "{dateFormat} {timeFormat} ~ {dateFormat} {timeFormat}". First date and time specifies the start date and second date and time specifies the end date. You can enter multiple ranges by separating them with a pipe. You can enter nothing to import all files.""");
        Print($"e.g. {new DateTime(2024, 1, 1).ToString($"{dateFormat} {timeFormat}")} ~ {new DateTime(2024, 1, 2, 23, 59, 59).ToString($"{dateFormat} {timeFormat}")} | {new DateTime(2024, 2, 1).ToString($"{dateFormat} {timeFormat}")} ~ {new DateTime(2024, 2, 2, 23, 59, 59).ToString($"{dateFormat} {timeFormat}")}");
        while (true)
        {
          var input = Console.ReadLine();
          if (input != null)
          {
            var ranges = input.Split('|');
            dateRanges = new List<(DateTimeOffset, DateTimeOffset)>(ranges.Length);
            for (var i = 0; i < ranges.Length; i++)
            {
              var range = ranges[i].Split('~', StringSplitOptions.TrimEntries);
              if (range.Length != 2)
              {
                PrintError("Invalid input.");
                dateRanges = null;
                break;
              }
              if (!DateTimeOffset.TryParseExact(range[0].Trim(), $"{dateFormat} {timeFormat}", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) || !DateTimeOffset.TryParseExact(range[1].Trim(), $"{dateFormat} {timeFormat}", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end) || start > end)
              {
                PrintError("Invalid input.");
                dateRanges = null;
                break;
              }
              if (start != end)
                dateRanges.Add((start, end));
            }
            if (dateRanges != null)
              break;
          }
          else
          {
            dateRanges = null;
            break;
          }
        }
      }
      if (dateRanges != null)
        dateRanges = DateRangesUtils.Merge(dateRanges);

      if (option.HasFlag(ImportOption.AddCustomPhotoExtension))
      {
        Print("""Enter the custom photo extensions you want to add. You can enter multiple extensions by separating them with a space. If you want to remove all default photo extensions, enter ":remove" and the custom extensions you want to use.""");
        Print($"Default photo extensions: {string.Join(' ', photoExtensions)}");
        var customPhotoExtension = Console.ReadLine();
        if (customPhotoExtension != null)
        {
          var entereds = customPhotoExtension.Split(' ').ToList();
          if (entereds.Contains(":remove"))
          {
            photoExtensions = [];
            entereds.Remove(":remove");
          }
          var customPhotoExtensions = entereds.Select(x => x.ToLower()).Select(x => x.StartsWith('.') ? x : "." + x);
          photoExtensions = [.. photoExtensions, .. customPhotoExtensions];
        }
      }
      if (option.HasFlag(ImportOption.AddCustomVideoExtension))
      {
        Print("""Enter the custom video extensions you want to add. You can enter multiple extensions by separating them with a space. If you want to remove all default video extensions, enter ":remove" and the custom extensions you want to use.""");
        Print($"Default video extensions: {string.Join(' ', videoExtensions)}");
        var customVideoExtension = Console.ReadLine();
        if (customVideoExtension != null)
        {
          var entereds = customVideoExtension.Split(' ').ToList();
          if (entereds.Contains(":remove"))
          {
            videoExtensions = [];
            entereds.Remove(":remove");
          }
          var customVideoExtensions = entereds.Select(x => x.ToLower()).Select(x => x.StartsWith('.') ? x : "." + x);
          videoExtensions = [.. videoExtensions, .. customVideoExtensions];
        }
      }

      if (option.HasFlag(ImportOption.ChangeDateInfoPriority))
      {
        Print("Sort the following in order of priority to get the shooting date time and enter the numbers separated by a space:");
        Print("1: Exif (Only for photoes)");
        Print("2: Media created (Only for videos. Media created doesn't include seconds, so if Media created and Creation or Modified are close, use Creation or Modified, otherwise use 0");
        Print("3: Creation");
        Print("4: Modified");
        Print("5: Access");
        Print("Default: Exif -> Media created -> Creation -> Modified -> Access");
        while (true)
        {
          var inputWayToGetShootingDateTime = Console.ReadLine();
          if (string.IsNullOrWhiteSpace(inputWayToGetShootingDateTime))
            continue;
          var ways = inputWayToGetShootingDateTime.Split(' ');
          if (ways.Length != 5 || MatchNotWayToGetShootingDateTime().IsMatch(inputWayToGetShootingDateTime) || ways.Distinct().Count() != 5)
          {
            PrintError("You must specify all options.");
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
    }

    var threadNumber = option.HasFlag(ImportOption.UseASingleThread) ? 1 : Environment.ProcessorCount;
    var startTime = DateTimeOffset.Now;

    Print("Searching directories for files...");
    Console.Out.Flush();

    var files = groupingMode switch
    {
      GroupingMode.NoGrouping => GetFilesNoGrouping(sourcePaths, excludedSourcePaths, destPaths, dateRanges, photoExtensions, videoExtensions, nameFormat.File, customFileFormat, conflictResolution, option, wayToGetShootingDateTime, threadNumber),
      GroupingMode.GroupedByYMD => GetFilesGroupedByYMD(sourcePaths, excludedSourcePaths, destPaths, dateRanges, photoExtensions, videoExtensions, nameFormat.DirectoryByYMD, customDirectoryFormatByYMD, nameFormat.File, customFileFormat, conflictResolution, option, wayToGetShootingDateTime, threadNumber),
      GroupingMode.GroupedByYD => GetFilesGroupedByYD(sourcePaths, excludedSourcePaths, destPaths, dateRanges, photoExtensions, videoExtensions, nameFormat.DirectoryByYD, customDirectoryFormatByYD, nameFormat.File, customFileFormat, conflictResolution, option, wayToGetShootingDateTime, threadNumber),
      GroupingMode.GroupedByD => GetFilesGroupedByD(sourcePaths, excludedSourcePaths, destPaths, dateRanges, photoExtensions, videoExtensions, nameFormat.DirectoryByD, customDirectoryFormatByD, nameFormat.File, customFileFormat, conflictResolution, option, wayToGetShootingDateTime, threadNumber),
      _ => throw new InvalidOperationException(),
    };

    if (files.Length <= 0)
    {
      Print("No files found.");
    }
    else
    {
      Print($"Found {files.Length} files.");

      Print("Creating directories...");
      GetDirectoriesFromFiles(files.Select(x => x.DestFilePath)).ForEach(directoryPath => Directory.CreateDirectory(directoryPath));

      Print($"{(option.HasFlag(ImportOption.Move) ? "Moving" : "Copying")} files...");
      var progressBar = new ProgressBar(50, files.Length);
      Console.Out.Flush();

      var isLog = option.HasFlag(ImportOption.Log);
      StringBuilder logString = new();
      StringBuilder errorLogString = new();

      files
        .AsParallel()
        .WithDegreeOfParallelism(threadNumber)
        .ForAll(file =>
        {
          try
          {
            var (hasThrownException, message) = CopyFile(file, option);
            lock (progressBar)
              progressBar.Update();
            if (isLog)
            {
              lock (logString)
                logString.AppendLine(message);
            }
            else if (hasThrownException)
            {
              lock (errorLogString)
                errorLogString.AppendLine(message);
            }
          }
          catch (Exception e)
          {
            lock (errorLogString)
              errorLogString.AppendLine($"\x1b[91m[Failed] {file.SourceFilePath} -> {file.DestFilePath}: {e.Message}\x1b[39m");
          }
        });
      progressBar.Done();


      if (isLog)
      {
        Console.WriteLine(logString.ToString());
      }
      else
      {
        var errorLog = logString.ToString();
        if (errorLog.Length > 0)
          Console.Error.WriteLine(errorLog);
      }
    }

    Print($"Elapsed time: {(DateTimeOffset.Now - startTime).TotalSeconds} s");

    Print("Press any key to exit.");
    Console.ReadKey();
  }
}
