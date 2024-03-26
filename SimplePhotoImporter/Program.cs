using System;
using System.IO;
using System.Text.RegularExpressions;

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

string fromPath;
while (true)
{
  Console.WriteLine("Enter the path to the folder you want to import from:");
  var inputFromPath = Console.ReadLine();

  if (inputFromPath == null || !Directory.Exists(inputFromPath))
    Console.WriteLine("The specified path does not exist.");
  else
  {
    fromPath = inputFromPath;
    break;
  }
}

string toPath;
while (true)
{
  Console.WriteLine("Enter the path to the folder you want to import to:");
  var inputToPath = Console.ReadLine();

  if (inputToPath == null || !Directory.Exists(inputToPath))
    Console.WriteLine("The specified path does not exist.");
  else
  {
    toPath = inputToPath;
    break;
  }
}

SeparateMode separateMode;
while (true)
{
  Console.WriteLine("Do you want to separate files by date? (y/n)");
  var inputSeparateMode = Console.ReadLine();

  if (inputSeparateMode == "y" || inputSeparateMode == "Y")
  {
    separateMode = SeparateMode.ByDate;
    break;
  }
  else if (inputSeparateMode == "n" || inputSeparateMode == "N")
  {
    separateMode = SeparateMode.ByNone;
    break;
  }
}


(Shell32.Folder folder, string? path)? folder = null;


Dictionary<int, Dictionary<int, Dictionary<int, List<(string, DateTimeOffset)>>>> GetFilesSeparatedByDate(string parentDirectory)
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
              dayGroup => dayGroup.Select(file => (file.Key, file.Value)).ToList())));
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
  ByDate,
  ByNone,
}