using SimplePhotoImporter.Usage.UsageItem;

namespace SimplePhotoImporter.Usage;

public class UsageText : IUsageItem
{
  public UsageText(string text)
  {
    Text = text;
  }
  public UsageText(string header, string text)
  {
    Header = header;
    Text = text;
  }

  public string? Header { get; set; } = null;
  public string Text { get; set; } = string.Empty;
}
