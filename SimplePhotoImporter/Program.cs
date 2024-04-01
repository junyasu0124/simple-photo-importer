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
    Console.WriteLine("Simple Photo Importer v" + version);

    string[] photoExtensions = [".jpeg", ".jpg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".ico", ".svg", ".heic"];
    string[] videoExtensions = [".mov", ".mp4", ".m2ts", ".mts", ".avi", ".flv"];

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

    if (data != null && !needInteractive)
    {
      (sourcePaths, excludedSourcePaths, destPaths, groupingMode, nameFormat, customFileFormat, customDirectoryFormatByYMD, customDirectoryFormatByYD, customDirectoryFormatByD, conflictResolution, option, photoExtensions, videoExtensions, wayToGetShootingDateTime) = data.Value;
    }

    if (needInteractive)
    {
      while (true)
      {
        Console.WriteLine("Enter the paths to the folder you want to import FROM. You can enter multiple paths by separating them with a space (use double quotes if the path contains a space).");
        Console.WriteLine("If you want to exclude a directory, enter the path with a asterisk at the beginning.");
        Console.WriteLine("e.g. \"C:\\Users\\user\\Pictures Folder\" *\"C:\\Users\\user\\Pictures Folder\\excluded\"");
        var inputFromPath = Console.ReadLine();

        var (message, paths, excludedPaths) = CheckInputDirectories(inputFromPath, true);
        if (message != null)
        {
          Console.Error.WriteLine(message);
          continue;
        }
        sourcePaths = paths ?? [];
        excludedSourcePaths = excludedPaths ?? [];
        break;
      }

      while (true)
      {
        Console.WriteLine("Enter the paths to the folder you want to import TO. You can enter multiple paths by separating them with a space (use double quotes if the path contains a space).");
        var inputToPath = Console.ReadLine();

        var (message, paths, _) = CheckInputDirectories(inputToPath, false);
        if (message != null)
        {
          Console.Error.WriteLine(message);
          continue;
        }
        destPaths = paths ?? [];
        break;
      }

      while (true)
      {
        Console.WriteLine("Enter the mode to for grouping files by directory:");
        Console.WriteLine("1: No grouping like \\IMG_0001.jpg");
        Console.WriteLine("2: By year, month, and day like \\2024\\01\\02\\IMG_0001.jpg");
        Console.WriteLine("3: By year and day like \\2024\\20240102\\IMG_0001.jpg");
        Console.WriteLine("4: By day like \\20240102\\IMG_0001.jpg");
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
          Console.Error.WriteLine("Invalid input.");
        }
      }

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
            Console.Error.WriteLine("Invalid input.");
            continue;
        }
        break;
      }
      switch (groupingMode)
      {
        case GroupingMode.GroupedByYMD:
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
                  nameFormat.DirectoryByYMD = DirectoryNameFormatByYMD.YMDNoSeparation;
                  break;
                case "2":
                  nameFormat.DirectoryByYMD = DirectoryNameFormatByYMD.CustomNameFormat;
                  customDirectoryFormatByYMD = SetCustomDirectoryFormatByYMD();
                  if (customDirectoryFormatByYMD == null)
                    return;
                  break;
                default:
                  Console.Error.WriteLine("Invalid input.");
                  continue;
              }
              break;
            }
            break;
          }
        case GroupingMode.GroupedByYD:
          {
            while (true)
            {
              Console.WriteLine("Enter the format of the directory name:");
              Console.WriteLine("1: Year\\YearMonthDay as yyyy\\yyyyMMdd");
              Console.WriteLine("2: Year\\Year_Month_Day as yyyy\\yyyy_MM_dd");
              Console.WriteLine("3: Year\\Year-Month-Day as yyyy\\yyyy-MM-dd");
              Console.WriteLine("4: Year\\MonthDay as yyyy\\MMdd");
              Console.WriteLine("5: Year\\Month_Day as yyyy\\MM_dd");
              Console.WriteLine("6: Year\\Month-Day as yyyy\\MM-dd");
              Console.WriteLine("7: Custom format");
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
                  Console.Error.WriteLine("Invalid input.");
                  continue;
              }
              break;
            }
            break;
          }
        case GroupingMode.GroupedByD:
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
                  Console.Error.WriteLine("Invalid input.");
                  continue;
              }
              break;
            }
            break;
          }
      }

      var isEnabledAddNumberToDirectory = groupingMode != GroupingMode.NoGrouping;
      while (true)
      {
        Console.WriteLine("Enter the conflict resolution. (It will be applied only to files that existed before importing. If multiple files with the same name are created in the same directory, a number will be added to the file name):");
        Console.WriteLine("1: Skip if the contents are the same, otherwise, add a number to the file name. (Recommended)");
        Console.WriteLine("2: Skip");
        Console.WriteLine("3: Skip by directory (All files which will be copied to the same directory will be skipped)");
        Console.WriteLine("4: Add a number to the file name");
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
          Console.WriteLine("5: Make a new directory having name with a number and add files to the directory" + addNumberDescription);
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
              Console.Error.WriteLine("Invalid input.");
              continue;
            }
            break;
          default:
            Console.Error.WriteLine("Invalid input.");
            continue;
        }
        break;
      }

      while (true)
      {
        Console.WriteLine("Enter the options you want to enable. You can enter multiple options by separating them with a space:");
        Console.WriteLine("1: Move instead of copy");
        Console.WriteLine("2: Make the extension lower case");
        Console.WriteLine("3: Make the extension upper case");
        Console.WriteLine("4: Add custom photo extension");
        Console.WriteLine("5: Add custom video extension");
        Console.WriteLine("6: Change priority of the way to get shooting date time");
        Console.WriteLine("7: Use a single thread");
        Console.WriteLine("8: Output log");
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
            option |= ImportOption.AddCustomPhotoExtension;
          else if (opt == "5")
            option |= ImportOption.AddCustomVideoExtension;
          else if (opt == "6")
            option |= ImportOption.ChangeDateInfoPriority;
          else if (opt == "7")
            option |= ImportOption.UseASingleThread;
          else if (opt == "8")
            option |= ImportOption.Log;
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

      if (option.HasFlag(ImportOption.AddCustomPhotoExtension))
      {
        Console.WriteLine("""Enter the custom photo extensions you want to add. You can enter multiple extensions by separating them with a space. If you want to remove the default photo extensions, enter ":remove" and the custom extensions you want to use.""");
        Console.WriteLine($"Default photo extensions: {string.Join(' ', photoExtensions)}");
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
        Console.WriteLine("""Enter the custom video extensions you want to add. You can enter multiple extensions by separating them with a space. If you want to remove the default video extensions, enter ":remove" and the custom extensions you want to use.""");
        Console.WriteLine($"Default video extensions: {string.Join(' ', videoExtensions)}");
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
        while (true)
        {
          Console.WriteLine("Sort the following in order of priority to get the shooting date time and enter the numbers separated by a space:");
          Console.WriteLine("1: Exif (Only for photoes)");
          Console.WriteLine("2: Media created (Only for videos. Media created doesn't include seconds, so if Media created and Creation or Modified are close, use Creation or Modified, otherwise use 0");
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
    }

    var threadCount = option.HasFlag(ImportOption.UseASingleThread) ? 1 : Environment.ProcessorCount;
    var startTime = DateTimeOffset.Now;

    Console.WriteLine("Searching directories for files...");

    var files = groupingMode switch
    {
      GroupingMode.NoGrouping => GetFilesNoGrouping(sourcePaths, excludedSourcePaths, destPaths, photoExtensions, videoExtensions, nameFormat.File, customFileFormat, conflictResolution, option, wayToGetShootingDateTime),
      GroupingMode.GroupedByYMD => GetFilesGroupedByYMD(sourcePaths, excludedSourcePaths, destPaths, photoExtensions, videoExtensions, nameFormat.DirectoryByYMD, customDirectoryFormatByYMD, nameFormat.File, customFileFormat, conflictResolution, option, wayToGetShootingDateTime),
      GroupingMode.GroupedByYD => GetFilesGroupedByYD(sourcePaths, excludedSourcePaths, destPaths, photoExtensions, videoExtensions, nameFormat.DirectoryByYD, customDirectoryFormatByYD, nameFormat.File, customFileFormat, conflictResolution, option, wayToGetShootingDateTime),
      GroupingMode.GroupedByD => GetFilesGroupedByD(sourcePaths, excludedSourcePaths, destPaths, photoExtensions, videoExtensions, nameFormat.DirectoryByD, customDirectoryFormatByD, nameFormat.File, customFileFormat, conflictResolution, option, wayToGetShootingDateTime),
      _ => throw new InvalidOperationException(),
    };

    if (files.Length <= 0)
    {
      Console.WriteLine("No files found.");
    }
    else
    {
      Console.WriteLine($"Found {files.Length} files.");

      Console.WriteLine("Creating directories...");
      GetDirectoriesFromFiles(files.Select(x => x.DestFilePath)).ForEach(directoryPath => Directory.CreateDirectory(directoryPath));

      Console.WriteLine($"{(option.HasFlag(ImportOption.Move) ? "Moving" : "Copying")} files...");
      var progressBar = new ProgressBar(50, files.Length);
      var isLog = option.HasFlag(ImportOption.Log);

      StringBuilder logString = new();
      StringBuilder errorLogString = new();

      files.AsParallelOrSingleAndForAll(file =>
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
          if (hasThrownException)
          {
            lock (errorLogString)
              errorLogString.AppendLine(message);
          }
        }
        catch (Exception e)
        {
          File.AppendAllText("D:\\error.txt", e.Message);
        }
      }, threadCount);
      progressBar.Done();

      var log = logString.ToString();
      var errorLog = logString.ToString();

      if (isLog)
        Console.WriteLine(log);
      else if (errorLog.Length > 0)
        Console.WriteLine(errorLog);
    }

    Console.WriteLine($"Elapsed time: {Math.Floor((DateTimeOffset.Now - startTime).TotalSeconds)} s");

    Console.WriteLine("Press any key to exit.");
    Console.ReadKey();
  }
}
