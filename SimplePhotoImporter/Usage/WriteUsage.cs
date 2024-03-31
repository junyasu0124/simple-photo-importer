using SimplePhotoImporter.Usage.UsageItem;

namespace SimplePhotoImporter.Usage;

public static partial class Usage
{
  public static void Write(IEnumerable<IUsageItem> items)
  {
    int? maxNameLength = null;
    int? childrenEndIndex = null;
    foreach (var (item, i) in items.Select((x, i) => (x, i)))
    {
      if (item is UsageChildren children)
      {
        if (maxNameLength.HasValue)
        {
          Console.WriteLine($"{children.Name.PadRight(maxNameLength.Value)} {children.Description}");
          if (i == childrenEndIndex)
          {
            maxNameLength = null;
            childrenEndIndex = null;
          }
        }
        else
        {
          var count = items.Count();
          for (var j = i + 1; j < count; j++)
          {
            if (items.ElementAt(j) is not UsageChildren)
            {
              maxNameLength = items.Take(i..j).Max(x => x.Name.Length);
              childrenEndIndex = j - 1;
              break;
            }
            if (j == count - 1)
            {
              maxNameLength = items.Take(i..).Max(x => x.Name.Length);
              childrenEndIndex = j;
            }
          }
          Console.WriteLine($"{children.Name.PadRight((maxNameLength ?? 0 as int?).Value)} {children.Description}");
        }
      }
      else if (item is UsageParent parent)
      {
        Console.WriteLine(parent.Name + ":");
        var maxlementLength = parent.Childrens.Max(x => x.Name.Length);
        foreach (var child in parent.Childrens)
        {
          Console.WriteLine($"  {child.Name.PadRight(maxlementLength)} {child.Description}");
        }
      }
      else
      {
        throw new InvalidOperationException();
      }
    }
  }
}
