﻿using SimplePhotoImporter.Usage.UsageItem;

namespace SimplePhotoImporter.Usage;

public class UsageChildren : IUsageString
{
  public UsageChildren(string name)
  {
    Name = name;
  }
  public UsageChildren(string name, string description)
  {
    Name = name;
    Description = description;
  }

  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
}
