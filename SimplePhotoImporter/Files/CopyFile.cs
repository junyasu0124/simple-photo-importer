namespace SimplePhotoImporter.Files;

public static partial class Files
{
  public static string CopyFile(FileAddress file, ImportOption option, ProgressBar progressBar)
  {
    var copyOrMove = option.HasFlag(ImportOption.Move) ? "move" : "copy";
    if (File.Exists(file.DestFilePath))
    {
      return $"Failed to {copyOrMove} {file.SourceFilePath} to {file.DestFilePath}: The file already exists.";
    }
    try
    {
      if (option.HasFlag(ImportOption.Move))
      {
        File.Move(file.SourceFilePath, file.DestFilePath, false);
        return $"Moved {file.SourceFilePath} to {file.DestFilePath}";
      }
      else
      {
        File.Copy(file.SourceFilePath, file.DestFilePath, false);
        return $"Copied {file.SourceFilePath} to {file.DestFilePath}";
      }
    }
    catch (IOException)
    {
      return $"Failed to {copyOrMove} {file.SourceFilePath} to {file.DestFilePath}: The file already exists.";
    }
    catch (Exception e)
    {
      return $"Failed to {copyOrMove} {file.SourceFilePath} to {file.DestFilePath}: {e.Message}";
    }
  }
}
