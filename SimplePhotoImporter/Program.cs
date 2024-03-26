Type? shellAppType = null;
dynamic? shell = null;


Console.WriteLine("Simple Photo Importer");

if (!OperatingSystem.IsWindows())
{
  Console.WriteLine("This program is only supported on Windows.");
  return;
}
shellAppType = Type.GetTypeFromProgID("Shell.Application");
if (shellAppType == null)
{
  Console.WriteLine("Failed to get Shell.Application COM object.");
  return;
}
shell = Activator.CreateInstance(shellAppType);
if (shell == null)
{
  Console.WriteLine("Failed to create Shell.Application COM object.");
  return;
}

string sourcePath;
while (true)
{
  Console.WriteLine("Enter the path to the folder you want to import from:");
  var inputFromPath = Console.ReadLine();

  if (inputFromPath == null || !Directory.Exists(inputFromPath))
    Console.WriteLine("The specified path does not exist.");
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
    Console.WriteLine("The specified path does not exist.");
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
  Console.WriteLine("1: By year, month, and day");
  Console.WriteLine("2: By day");
  Console.WriteLine("3: By none");
  var inputSeparateMode = Console.ReadLine();

  if (inputSeparateMode == "1")
  {
    separateMode = SeparateMode.ByYearAndMonthAndDay;
    break;
  }
  else if (inputSeparateMode == "2")
  {
    separateMode = SeparateMode.ByDay;
    break;
  }
  else if (inputSeparateMode == "3")
  {
    separateMode = SeparateMode.ByNone;
    break;
  }
  else if (inputSeparateMode == null)
  {
    continue;
  }
  else
  {
    Console.WriteLine("Invalid input.");
  }
}


(Shell32.Folder folder, string? path)? folder = null;
if (separateMode == SeparateMode.ByNone)
{
  var files = GetFiles(sourcePath);
  foreach (var file in files)
  {
    var fileName = Path.GetFileName(file.Key);
    var newFilePath = Path.Combine(destPath, fileName);
    File.Move(file.Key, newFilePath);
  }
}
else if (separateMode == SeparateMode.ByYearAndMonthAndDay)
{
  var files = GetFilesSeparatedByYearAndMonthAndDay(sourcePath);
  foreach (var year in files)
  {
    foreach (var month in year.Value)
    {
      foreach (var day in month.Value)
      {
        var directoryPath = Path.Combine(destPath, year.Key.ToString(), month.Key.ToString(), day.Key.ToString());
        Directory.CreateDirectory(directoryPath);

        foreach (var (path, date) in day.Value)
        {
          var fileName = Path.GetFileName(path);
          var newFilePath = Path.Combine(directoryPath, fileName);
          File.Copy(path, newFilePath);
        }
      }
    }
  }
}
else if (separateMode == SeparateMode.ByDay)
{
  var files = GetFilesSeparatedByDay(sourcePath);
  foreach (var day in files)
  {
    var directoryPath = Path.Combine(destPath, day.Key.ToString("yyyyMMdd"));
    Directory.CreateDirectory(directoryPath);

    foreach (var (path, date) in day.Value)
    {
      var fileName = Path.GetFileName(path);
      var newFilePath = Path.Combine(directoryPath, fileName);
      File.Copy(path, newFilePath);
    }
  }
}


Dictionary<int, Dictionary<int, Dictionary<int, (string path, DateTimeOffset date)[]>>> GetFilesSeparatedByYearAndMonthAndDay(string parentDirectory)
{
  var files = GetFiles(parentDirectory);

  return files
    .GroupBy(file => file.Value.Year)
    .ToDictionary(yearGroup => yearGroup.Key,
      yearGroup => yearGroup
        .GroupBy(file => file.Value.Month)
        .ToDictionary(monthGroup => monthGroup.Key,
          monthGroup => monthGroup.GroupBy(file => file.Value.Day)
            .ToDictionary(dayGroup => dayGroup.Key,
              dayGroup => dayGroup.Select(file => (file.Key, file.Value)).ToArray())));
}
Dictionary<DateOnly, (string path, DateTimeOffset date)[]> GetFilesSeparatedByDay(string parentDirectory)
{
  var files = GetFiles(parentDirectory);

  return files
    .GroupBy(file => new DateOnly(file.Value.Year, file.Value.Month, file.Value.Day))
    .ToDictionary(dayGroup => dayGroup.Key,
      dayGroup => dayGroup.Select(file => (file.Key, file.Value)).ToArray());
}
Dictionary<string, DateTimeOffset> GetFiles(string parentDirectory)
{
  string[] pictureExtensions = [".jpeg", ".jpg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".ico", ".svg", ".heic"];
  string[] movieExtensions = [".mov", ".mp4", ".m2ts", ".mts", ".avi", ".flv"];
  string[] extensions = [.. pictureExtensions, .. movieExtensions];

  return Directory.EnumerateFiles(parentDirectory, "*.*", SearchOption.AllDirectories)
      .Where(file => extensions.Contains(Path.GetExtension(file).ToLower()))
      .ToDictionary(file => file, GetShootingDate);
}
DateTimeOffset GetShootingDate(string filePath)
{
  if (folder == null || folder.Value.path != Path.GetDirectoryName(filePath))
    folder = (shell.NameSpace(Path.GetDirectoryName(filePath)), Path.GetDirectoryName(filePath));
  Shell32.FolderItem folderItem = folder.Value.folder.ParseName(Path.GetFileName(filePath));

  var dateString = folder.Value.folder.GetDetailsOf(folderItem, 12);
  DateTimeOffset? date = null;
  if (dateString != "")
  {
    dateString = dateString.Replace("‎", "");
    dateString = dateString.Replace("‏", "");
    var retarray = dateString.Split([' ', '/', ':']).ToList();
    date = new DateTime(int.Parse(retarray[0]), int.Parse(retarray[1]), int.Parse(retarray[2]), int.Parse(retarray[3]), int.Parse(retarray[4]), 0);
  }
  if (date == null)
  {
    date = new DateTimeOffset(File.GetCreationTime(filePath));
  }
  return date.Value;
}

enum SeparateMode
{
  ByNone,
  ByYearAndMonthAndDay,
  ByDay,
}

class Progress(int width, int parMax)
{
  public int columns = Console.WindowWidth;
  public int width = width;
  public int par = 0;
  public int parMax = parMax;
  protected int rowLate = Console.CursorTop;

  public virtual void Update(string message)
  {
    int row0 = Console.CursorTop;

    float parcent = (float)par / parMax;
    int widthNow = (int)Math.Floor(width * parcent);

    string gauge = new string('>', widthNow) + new string(' ', width - widthNow);
    string status = $"({parcent * 100:f1}%<-{par}/{parMax})";

    Console.Error.WriteLine($"#[{gauge}]#{status}");
    ClearScreenDown();

    Console.Error.WriteLine(message);
    rowLate = Console.CursorTop;
    Console.SetCursorPosition(0, row0);
    par++;
  }

  public virtual void Done(string doneAlert)
  {
    int sideLen = (int)Math.Floor((float)(width - doneAlert.Length) / 2);

    string gauge = new string('=', sideLen) + doneAlert;
    gauge += new string('=', width - gauge.Length);
    string status = $"(100%<-{parMax}/{parMax})";

    ClearScreenDown();
    Console.Error.WriteLine($"#[{gauge}]#{status}");
  }

  protected void ClearScreenDown()
  {
    int clearRange = rowLate - (Console.CursorTop - 1);
    Console.Error.Write(new string(' ', columns * clearRange));
    Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - clearRange);
  }
}
