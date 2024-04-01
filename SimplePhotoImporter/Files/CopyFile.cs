namespace SimplePhotoImporter.Files;

public static partial class Files
{
  public static (bool HasThrownException, string Message) CopyFile(FileAddress file, ImportOption option)
  {
    if (File.Exists(file.DestFilePath))
    {
      return (true, $"[Failed] {file.SourceFilePath} -> {file.DestFilePath}: The file already exists.");
    }
    try
    {
      if (option.HasFlag(ImportOption.Move))
      {
        File.Move(file.SourceFilePath, file.DestFilePath, false);
        return (false, $"[Moved] {file.SourceFilePath} -> {file.DestFilePath}");
      }
      else
      {
        File.Copy(file.SourceFilePath, file.DestFilePath, false);
        return (false, $"[Copied] {file.SourceFilePath} -> {file.DestFilePath}");
      }
    }
    catch (IOException)
    {
      return (true, $"[Failed] {file.SourceFilePath} -> {file.DestFilePath}: The file already exists.");
    }
    catch (Exception e)
    {
      return (true, $"[Failed] {file.SourceFilePath} -> {file.DestFilePath}: {e.Message}");
    }
  }
}
