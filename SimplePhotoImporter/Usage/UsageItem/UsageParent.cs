using SimplePhotoImporter.Usage.UsageItem;

namespace SimplePhotoImporter.Usage;

public class UsageParent : IUsageString
{
  public UsageParent(string name, IEnumerable<UsageChildren> childrens)
  {
    Name = name;
    Childrens = childrens.ToList();
  }

  public string Name { get; set; } = string.Empty;
  public List<UsageChildren> Childrens { get; set; } = [];
}
