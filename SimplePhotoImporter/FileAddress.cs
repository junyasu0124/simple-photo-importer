namespace SimplePhotoImporter;
public class FileAddress(string sourceFilePath, string destFilePath, DateTimeOffset date)
{
  public string SourceFilePath { get; set; } = sourceFilePath;
  public string DestFilePath { get; set; } = destFilePath;
  public DateTimeOffset Date { get; set; } = date;
}
