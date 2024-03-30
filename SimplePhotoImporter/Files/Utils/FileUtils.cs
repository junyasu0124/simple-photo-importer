using Shell32;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using static SimplePhotoImporter.CustomRegexes;

#pragma warning disable CA1416 // プラットフォームの互換性を検証

namespace SimplePhotoImporter.Files.Utils;

public static class FileUtils
{
  private static readonly dynamic shell;
  private static string? objFolderPath = null;
  private static Folder? objFolder = null;

  static FileUtils()
  {
    Type? shellAppType = Type.GetTypeFromProgID("Shell.Application") ?? throw new InvalidOperationException("Failed to get the type of Shell.Application.");
    var createdShell = Activator.CreateInstance(shellAppType);
    if (createdShell == null)
    {
      throw new InvalidOperationException("Failed to create an instance of Shell.Application.");
    }
    else
    {
      shell = createdShell;
    }
  }

  public static DateTimeOffset GetShootingDate(string filePath, string[] pictureExtensions, string[] movieExtensions, WayToGetShootingDateTime[] wayToGetShootingDateTime)
  {
    var extension = Path.GetExtension(filePath).ToLower();
    var isMovie = movieExtensions.Contains(extension);
    if (!isMovie)
    {
      if (!pictureExtensions.Contains(extension))
        throw new ArgumentException("The file is not a picture or a movie.");
    }
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
        FolderItem folderItem = objFolder.ParseName(Path.GetFileName(filePath));
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

  public static List<string> GetDirectoriesFromFiles(IEnumerable<string> files)
  {
    return files.Select(Path.GetDirectoryName).Distinct().Where(x => x != null).ToList() as List<string> ?? [];
  }

  public static bool IsSameFile(string file1, string file2)
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
}
