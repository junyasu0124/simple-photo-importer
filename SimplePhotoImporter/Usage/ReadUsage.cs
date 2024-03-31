using System.Text;
using System.Text.RegularExpressions;
using static SimplePhotoImporter.Checks.Checks;

namespace SimplePhotoImporter.Usage;

public static partial class Usage
{
  public static (
    bool CanContinue,
    bool NeedInteractive,
    (string[] SourcePaths,
    string[] ExcludedSourcePaths,
    string[] DestPaths,
    GroupingMode GroupingMode,
    (FileNameFormat File, DirectoryNameFormatByYMD DirectoryByYMD, DirectoryNameFormatByYD DirectoryByYD, DirectoryNameFormatByD DirectoryByD) NameFormat,
    string? CustomFileFormat,
    (string Year, string Month, string Day)? CustomDirectoryFormatByYMD,
    (string Year, string Day)? CustomDirectoryFormatByYD,
    string? CustomDirectoryFormatByD,
    ConflictResolution ConflictResolution,
    ImportOption Option,
    string[] PhotoExtensions,
    string[] VideoExtensions,
    WayToGetShootingDateTime[] WayToGetShootingDateTime)? Data
    ) Read(string[] args, string[] photoExtensions, string[] videoExtensions, WayToGetShootingDateTime[] wayToGetShootingDateTime)
  {
    string[] arguments =
    [
      "--source-paths=",
      "--dest-paths=",
      "--grouping-mode=",
      "--file-name-format=",
      "--directory-name-format=",
      "--conflict-resolution="
    ];
    string[] options =
    [
      "--move",
      "--lower-extension",
      "--upper-extension",
      "--custom-photo-extension=",
      "--custom-video-extension=",
      "--date-info-priority=",
      "--single-thread",
    ];

    if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h"))
    {
      Write(
        [
          new UsageText("This program searches for photoes and videos in a specified directory, separates the files by shooting date and time, renames them, and imports them into other directories."),
          new UsageLineBreak(),

          new UsageText("Usage:", "SimplePhotoImporter <-Start the program in interactive mode"),
          new UsageText("Usage:", "SimplePhotoImporter [app-option]"),
          new UsageText("Usage:", $"SimplePhotoImporter [{arguments[0]}...] [{arguments[1]}...] ([{arguments[2]}...]) ([{arguments[3]}...]) ([{arguments[4]}...]) ([{arguments[5]}...]) ([{nameof(options)}])"),
          new UsageLineBreak(),

          new UsageParent("app-option",
            [
              new UsageChildren("--help, -h", "Show this help message"),
              new UsageChildren("--version, -v", "Show the version of this tool"),
            ]
          ),
          new UsageLineBreak(),
          new UsageChildren($"{arguments[0]}...", "The paths to the folder you want to import from. You can enter multiple paths by separating them with a space (use double quotes if the path contains a space). If you want to exclude a directory, enter the path with a asterisk at the beginning. e.g. \"C:\\Users\\user\\Pictures Folder\" *\"C:\\Users\\user\\Pictures Folder\\excluded\""),
          new UsageChildren($"{arguments[1]}...", "The paths to the folder you want to import to. You can enter multiple paths by separating them with a space (use double quotes if the path contains a space)."),
          new UsageChildren($"{arguments[2]}...", "The mode to for grouping files by directory. 1({space}): \\IMG_0001.jpg, 2(y\\M\\d): \\yyyy\\MM\\dd\\IMG_0001.jpg, 3(y\\d): \\yyyy\\dd\\IMG_0001.jpg, 4(d): \\dd\\IMG_0001.jpg  <Default: 1>"),
          new UsageChildren($"{arguments[3]}...", "The format of the file name. Enter the format like yyyyMMddHHmmss. <Default: {OriginalFileName}>"),
          new UsageChildren($"{arguments[4]}...", "The format of the directory name by year, month, and day. Enter the format like yyyy\\MM\\dd. You must specify {--grouping-mode=2: three, --grouping-mode=3: two, --grouping-mode=4: one} values separated by a backslash."),
          new UsageChildren($"{arguments[5]}...", "The conflict resolution. 1: Skip if the contents are the same, otherwise, add a number to the file name. 2: Skip. 3: Skip by directory. 4: Add a number to the file name. 5: Make a new directory having name with a number and add files to the directory. <Default: 1>"),
          new UsageLineBreak(),
          new UsageParent($"{nameof(options)}",
            [
              new UsageChildren(options[0], "Move instead of copy"),
              new UsageChildren(options[1], "Make the extension lower case"),
              new UsageChildren(options[2], "Make the extension upper case"),
              new UsageChildren($"{options[3]}...", "Add custom photo extension (e.g. --custom-photo-extension=:remove(<-Remove the default extensions) .photo)"),
              new UsageChildren($"{options[4]}...", "Add custom video extension (e.g. --custom-video-extension=:remove(<-Remove the default extensions) .video)"),
              new UsageChildren($"{options[5]}...", "Change priority of the way to get shooting date time. 1: Exif, 2: Media created, 3: Creation, 4: Modified, 5: Access  You must specify all values separated by a space."),
              new UsageChildren(options[6], "Use a single thread"),
            ]
          ),
        ]);
      return (false, false, null);
    }
    else if (args.Length > 0 && (args[0] == "--version" || args[0] == "-v"))
    {
      var version = typeof(Program).Assembly.GetName().Version;
      Console.WriteLine("Simple Photo Importer v" + version);
      return (false, false, null);
    }
    else if (args.Length == 0)
    {
      return (true, true, null);
    }
    else
    {
      var i = 0;
      var startI = i;

      List<string> sourcePaths = [];
      List<string> excludedSourcePaths = [];
      if (!args[0].StartsWith(arguments[0]))
      {
        Console.WriteLine("The first argument must be --source-paths=...");
        return (false, false, null);
      }

      StringBuilder pathsString = new();
      for (i = 0; i < args.Length; i++)
      {
        if (args[i].StartsWith(arguments[1]))
          break;
        var path = " \"" + (i == 0 ? Regex.Replace(args[0], $"^{arguments[0]}", "") : " " + args[i]) + "\"";
        pathsString.Append(path);
      }
      var (pathMessage, paths, excludedPaths) = CheckInputDirectories(pathsString.ToString(), true);
      if (pathMessage != null)
      {
        Console.WriteLine(pathMessage);
        return (false, false, null);
      }
      sourcePaths = [.. paths];
      excludedSourcePaths = [.. excludedPaths];

      List<string> destPaths = [];

      startI = i;
      pathsString = new();
      for (i += 0; i < args.Length; i++)
      {
        if (args[i].StartsWith(arguments[0]) || (args[i].StartsWith(arguments[1]) && i != startI))
        {
          Console.WriteLine("The argument is duplicated.");
          return (false, false, null);
        }
        else if (arguments[2..].Any(x => args[i].StartsWith(x)))
          break;
        pathsString.Append(" \"" + (i == startI ? Regex.Replace(args[startI], $"^{arguments[1]}", "") : args[i]) + "\"");
      }
      (pathMessage, paths, _) = CheckInputDirectories(pathsString.ToString(), false);
      if (pathMessage != null)
      {
        Console.WriteLine(pathMessage);
        return (false, false, null);
      }
      destPaths = [.. paths];

      GroupingMode groupingMode = GroupingMode.NoGrouping;
      (FileNameFormat File, DirectoryNameFormatByYMD DirectoryByYMD, DirectoryNameFormatByYD DirectoryByYD, DirectoryNameFormatByD DirectoryByD) nameFormat = (0, 0, 0, 0);
      string? customFileFormat = null;
      (string Year, string Month, string Day)? customDirectoryFormatByYMD = null;
      (string Year, string Day)? customDirectoryFormatByYD = null;
      string? customDirectoryFormatByD = null;
      ConflictResolution conflictResolution = ConflictResolution.SkipIfSame;

      string? customDirectoryFormatTemp = null;

      for (i += 0; i < args.Length; i++)
      {
        // --source-paths= --dest-paths
        if (args[i].StartsWith(arguments[0]) || args[i].StartsWith(arguments[1]))
        {
          Console.WriteLine("The argument is duplicated.");
          return (false, false, null);
        }
        // --grouping-mode
        else if (args[i].StartsWith(arguments[2]))
        {
          var inputGroupingMode = args[i].Replace(arguments[2], "") switch
          {
            "2" or "y\\M\\d" => GroupingMode.GroupedByYMD,
            "3" or "y\\d" => GroupingMode.GroupedByYD,
            "4" or "d" => GroupingMode.GroupedByD,
            var other => CheckOther(other),
          };
          static GroupingMode? CheckOther(string other)
          {
            if (other == "1" || string.IsNullOrEmpty(other)) return GroupingMode.NoGrouping;
            return null;
          }
          if (inputGroupingMode == null)
          {
            Console.WriteLine("--grouping-mode=... is invalid. Please enter 1, 2, 3, 4, {space}, y\\M\\d, y\\d, or d.");
            return (false, false, null);
          }
          else groupingMode = inputGroupingMode.Value;
        }
        // --file-name-format
        else if (args[i].StartsWith(arguments[3]))
        {
          var inputFileNameFormat = args[i][arguments[3].Length..];
          nameFormat.File = FileNameFormat.CustomNameFormat;
          customFileFormat = Regex.Replace(inputFileNameFormat, $"^{arguments[3]}", "");
          for (i += 1; i < args.Length; i++)
          {
            if (arguments.Where(x => x != arguments[3]).Any(x => args[i].StartsWith(x)))
            {
              i -= 1;
              break;
            }
            customFileFormat += " " + args[i];
          }
          customFileFormat = customFileFormat.Trim('"');
          if (!IsValidDateTimeFormat(customFileFormat))
          {
            Console.WriteLine("--file-name-format=... is invalid.");
            return (false, false, null);
          }
        }
        // --directory-name-format
        else if (args[i].StartsWith(arguments[4]))
        {
          var inputDirectoryNameFormat = args[i][arguments[4].Length..];
          customDirectoryFormatTemp = Regex.Replace(inputDirectoryNameFormat, $"^{arguments[4]}", "");
          for (i += 1; i < args.Length; i++)
          {
            if (arguments.Where(x => x != arguments[4]).Any(x => args[i].StartsWith(x)))
            {
              i -= 1;
              break;
            }
            customDirectoryFormatTemp += " " + args[i];
          }
          customDirectoryFormatTemp = customDirectoryFormatTemp.Trim('"');
          if (customDirectoryFormatTemp.Split('\\').Any(x => !IsValidDateTimeFormat(x)))
          {
            Console.WriteLine("--directory-name-format=... is invalid.");
            return (false, false, null);
          }
        }
        // --conflict-resolution
        else if (args[i].StartsWith(arguments[5]))
        {
          ConflictResolution? inputConflictResolution = args[i].Replace(arguments[5], "") switch
          {
            "1" => ConflictResolution.SkipIfSame,
            "2" => ConflictResolution.Skip,
            "3" => ConflictResolution.SkipByDirectory,
            "4" => ConflictResolution.AddNumber,
            "5" => ConflictResolution.AddNumberToDirectory,
            var other => null,
          };
          if (inputConflictResolution == null)
          {
            Console.WriteLine("--conflict-resolution=... is invalid. Please enter 1, 2, 3, 4, or 5.");
            return (false, false, null);
          }
          else conflictResolution = inputConflictResolution.Value;
        }
        else if (options.Any(x => args[i].StartsWith(x)))
          break;
        else
        {
          Console.WriteLine("The argument is invalid.");
          return (false, false, null);
        }
      }

      if (customDirectoryFormatTemp != null)
      {
        if (groupingMode == GroupingMode.GroupedByYMD)
        {
          var inputDirectoryNameFormatByYMD = customDirectoryFormatTemp.Split("\\");
          if (inputDirectoryNameFormatByYMD.Length != 3)
          {
            Console.WriteLine("--directory-name-format=... is invalid. Please enter the format like yyyy\\MM\\dd.");
            return (false, false, null);
          }
          customDirectoryFormatByYMD = (inputDirectoryNameFormatByYMD[0], inputDirectoryNameFormatByYMD[1], inputDirectoryNameFormatByYMD[2]);
          nameFormat.DirectoryByYMD = DirectoryNameFormatByYMD.CustomNameFormat;
        }
        else if (groupingMode == GroupingMode.GroupedByYD)
        {
          var inputDirectoryNameFormatByYD = customDirectoryFormatTemp.Split("\\");
          if (inputDirectoryNameFormatByYD.Length != 2)
          {
            Console.WriteLine("--directory-name-format=... is invalid. Please enter the format like yyyy\\dd.");
            return (false, false, null);
          }
          customDirectoryFormatByYD = (inputDirectoryNameFormatByYD[0], inputDirectoryNameFormatByYD[1]);
          nameFormat.DirectoryByYD = DirectoryNameFormatByYD.CustomNameFormat;
        }
        else if (groupingMode == GroupingMode.GroupedByD)
        {
          var inputDirectoryNameFormatByD = customDirectoryFormatTemp.Split("\\");
          if (inputDirectoryNameFormatByD.Length != 1)
          {
            Console.WriteLine("--directory-name-format=... is invalid. Please enter the format like yyyyMMdd.");
            return (false, false, null);
          }
          customDirectoryFormatByD = customDirectoryFormatTemp;
          nameFormat.DirectoryByD = DirectoryNameFormatByD.CustomNameFormat;
        }
        else
        {
          Console.WriteLine("You cannot specify --directory-name-format=... when --grouping-mode=1.");
        }
      }
      if (groupingMode == GroupingMode.NoGrouping && conflictResolution == ConflictResolution.AddNumberToDirectory)
      {
        Console.WriteLine("You cannot specify --conflict-resolution=5 when --grouping-mode=1.");
        return (false, false, null);
      }

      ImportOption option = 0;

      for (i += 0; i < args.Length; i++)
      {
        // --move
        if (args[i].StartsWith(options[0]))
        {
          option |= ImportOption.Move;
        }
        // --lower-extension
        else if (args[i].StartsWith(options[1]))
        {
          option |= ImportOption.MakeExtensionLower;
        }
        // --upper-extension
        else if (args[i].StartsWith(options[2]))
        {
          option |= ImportOption.MakeExtensionUpper;
        }
        // --custom-photo-extension
        else if (args[i].StartsWith(options[3]))
        {
          option |= ImportOption.AddCustomPhotoExtension;
          List<string> extensions = [];
          startI = i;
          for (i += 0; i < args.Length; i++)
          {
            if (options.Where(x => x != options[3]).Any(x => args[i].StartsWith(x)))
            {
              i -= 1;
              break;
            }
            if (startI == i)
              extensions.Add(Regex.Replace(args[startI], $"^{options[3]}", ""));
            else
              extensions.Add(args[i]);
          }
          if (extensions.Contains(":remove"))
          {
            photoExtensions = [];
            extensions.Remove(":remove");
          }
          photoExtensions = [.. photoExtensions, .. extensions.Select(x => x.StartsWith('.') ? x : "." + x)];
        }
        // --custom-video-extension
        else if (args[i].StartsWith(options[4]))
        {
          option |= ImportOption.AddCustomVideoExtension;
          List<string> extensions = [];
          startI = i;
          for (i += 0; i < args.Length; i++)
          {
            if (options.Where(x => x != options[4]).Any(x => args[i].StartsWith(x)))
            {
              i -= 1;
              break;
            }
            if (startI == i)
              extensions.Add(Regex.Replace(args[startI], $"^{options[4]}", ""));
            else
              extensions.Add(args[i]);
          }
          if (extensions.Contains(":remove"))
          {
            videoExtensions = [];
            extensions.Remove(":remove");
          }
          videoExtensions = [.. videoExtensions, .. extensions.Select(x => x.StartsWith('.') ? x : "." + x)];
        }
        // --date-info-priority
        else if (args[i].StartsWith(options[5]))
        {
          option |= ImportOption.ChangeDateInfoPriority;
          List<WayToGetShootingDateTime?> priorities = [];
          startI = i;
          for (i += 0; i < args.Length; i++)
          {
            if (options.Where(x => x != options[5]).Any(x => args[i].StartsWith(x)))
            {
              i -= 1;
              break;
            }
            priorities.Add((startI == i ? Regex.Replace(args[startI], $"^{options[5]}", "") : args[i]) switch
            {
              "1" => WayToGetShootingDateTime.Exif,
              "2" => WayToGetShootingDateTime.MediaCreated,
              "3" => WayToGetShootingDateTime.Creation,
              "4" => WayToGetShootingDateTime.Modified,
              "5" => WayToGetShootingDateTime.Access,
              _ => null,
            });
          }
          if (priorities.Contains(null))
          {
            Console.WriteLine("--date-info-priority=... is invalid. Please enter 1, 2, 3, 4, 5.");
            return (false, false, null);
          }
          wayToGetShootingDateTime = priorities.Select(x => x!.Value).ToArray();
        }
        // --single-thread
        else if (args[i].StartsWith(options[6]))
        {
          option |= ImportOption.UseASingleThread;
        }
        else
        {
          Console.WriteLine("The argument is invalid.");
          return (false, false, null);
        }
      }

      return (true, false, ([.. sourcePaths], [.. destPaths], [.. destPaths], groupingMode, nameFormat, customFileFormat, customDirectoryFormatByYMD, customDirectoryFormatByYD, customDirectoryFormatByD, conflictResolution, option, photoExtensions, videoExtensions, wayToGetShootingDateTime));
    }
  }
}
