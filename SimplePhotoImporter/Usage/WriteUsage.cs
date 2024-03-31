using SimplePhotoImporter.Usage.UsageItem;
using System.Text;

namespace SimplePhotoImporter.Usage;

public static partial class Usage
{
  private static readonly char[] splitChars = ['\n', '\r', ' ',];

  public static void Write(IEnumerable<IUsageItem> usageItems)
  {
    int? maxNameLength = null;
    int? childrenEndIndex = null;
    foreach (var (usageItem, i) in usageItems.Select((x, i) => (x, i)))
    {
      if (usageItem is UsageLineBreak)
      {
        Console.WriteLine();
        continue;
      }
      else if (usageItem is UsageText usageText)
      {
        int consoleWidth = Console.WindowWidth - 15;
        var headerLength = (usageText.Header ?? string.Empty).Length;
        int descriptionLength = Math.Max(consoleWidth - headerLength, 20);
        var lines = usageText.Text.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
        var description = new StringBuilder();
        var header = usageText.Header == null ? string.Empty : usageText.Header.PadRight(headerLength) + " ";
        var isFirst = true;
        foreach (var line in lines)
        {
          if (description.Length + line.Length + 1 > descriptionLength)
          {
            if (isFirst)
            {
              Console.WriteLine($"{header}{description}");
              isFirst = false;
            }
            else
              Console.WriteLine($"{string.Empty.PadRight(header.Length)}{description}");
            description.Clear();
          }
          description.Append(line + " ");
        }
        if (isFirst)
          Console.WriteLine($"{header}{description}");
        else
          Console.WriteLine($"{string.Empty.PadRight(header.Length)}{description}");
        continue;
      }

      var item = usageItem as IUsageString;
      if (item is UsageChildren children)
      {
        if (maxNameLength.HasValue)
        {
          WriteChildren(maxNameLength.Value, children, false);
          if (i == childrenEndIndex)
          {
            maxNameLength = null;
            childrenEndIndex = null;
          }
        }
        else
        {
          var count = usageItems.Count();
          for (var j = i + 1; j < count; j++)
          {
            if (usageItems.ElementAt(j) is not UsageChildren)
            {
              maxNameLength = usageItems.Take(i..j).Max(x =>
              {
                if (x is not IUsageString usageString)
                  return 0;
                return usageString.Name.Length;
              });
              childrenEndIndex = j - 1;
              break;
            }
            if (j == count - 1)
            {
              maxNameLength = usageItems.Take(i..).Max(x =>
              {
                if (x is not IUsageString usageString)
                  return 0;
                return usageString.Name.Length;
              });
              childrenEndIndex = j;
            }
          }
          WriteChildren(maxNameLength!.Value, children, false);
        }
      }
      else if (item is UsageParent parent)
      {
        Console.WriteLine(parent.Name + ":");
        var maxlementLength = parent.Childrens.Max(x => x.Name.Length);
        foreach (var child in parent.Childrens)
        {
          WriteChildren(maxlementLength, child, true);
        }
      }
      else
      {
        throw new InvalidOperationException();
      }
    }

    static void WriteChildren(int maxNameLength, UsageChildren children, bool doIndent)
    {
      int consoleWidth = Console.WindowWidth - 12;
      int descriptionLength = Math.Max(consoleWidth - maxNameLength, 20);
      var lines = children.Description.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
      var description = new StringBuilder();
      var indention = doIndent ? "  " : string.Empty;
      var isFirst = true;
      foreach (var line in lines)
      {
        if (description.Length + line.Length + 1 > descriptionLength)
        {
          if (isFirst)
          {
            Console.WriteLine($"{indention}{children.Name.PadRight(maxNameLength)} {description}");
            isFirst = false;
          }
          else
            Console.WriteLine($"{indention}{string.Empty.PadRight(maxNameLength)} {description}");
          description.Clear();
        }
        description.Append(line + " ");
      }
      if (isFirst)
        Console.WriteLine($"{indention}{children.Name.PadRight(maxNameLength)} {description}");
      else
        Console.WriteLine($"{indention}{string.Empty.PadRight(maxNameLength)} {description}");
    }
  }
}
