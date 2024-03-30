namespace SimplePhotoImporter.Files;

public static partial class Files
{
  public static void CopyFile(FileAddress file, ImportOption option)
  {
    if (File.Exists(file.DestFilePath))
    {
      DisplayFailed();
      return;
    }
    try
    {
      if (option.HasFlag(ImportOption.Move))
        File.Move(file.SourceFilePath, file.DestFilePath, false);
      else
        File.Copy(file.SourceFilePath, file.DestFilePath, false);
    }
    catch (IOException)
    {
      DisplayFailed();
    }
    catch (Exception e)
    {
      Console.Error.WriteLine($"Failed to copy {file.SourceFilePath} to {file.DestFilePath}: {e.Message}");
    }

    void DisplayFailed()
    {
      Console.Error.WriteLine($"Failed to copy {file.SourceFilePath} to {file.DestFilePath}: The file already exists.");
    }
  }
}
