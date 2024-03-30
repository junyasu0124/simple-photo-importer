namespace SimplePhotoImporter.Checks;

public static partial class Checks
{
  public static string? SetCustomFileFormat()
  {
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the file name:");
      var usingSpecifier = FormatSpecifier.Year | FormatSpecifier.Month | FormatSpecifier.Day | FormatSpecifier.Time | FormatSpecifier.FileName;
      if (i == 0)
        OutputCustomFormatDescription(usingSpecifier);
      var customFileFormat = Console.ReadLine();
      if (customFileFormat != null && IsValidFormat(customFileFormat))
        return customFileFormat;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    return null;
  }
  public static string? SetCustomDirectoryFormatByDay()
  {
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the directory name:");
      var usingSpecifier = FormatSpecifier.Year | FormatSpecifier.Month | FormatSpecifier.Day;
      if (i == 0)
        OutputCustomFormatDescription(usingSpecifier);
      var customDirectoryFormatByDay = Console.ReadLine();
      if (customDirectoryFormatByDay != null && IsValidFormat(customDirectoryFormatByDay))
        return customDirectoryFormatByDay;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    return null;
  }
  public static (string Year, string Month, string Day)? SetCustomDirectoryFormatByYMD()
  {
    string? year = "", month = "", day = "";
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the year directory name:");
      var usingSpecifier = FormatSpecifier.Year | FormatSpecifier.Month | FormatSpecifier.Day;
      if (i == 0)
        OutputCustomFormatDescription(usingSpecifier);
      year = Console.ReadLine();
      if (year != null && IsValidFormat(year))
        break;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the month directory name:");
      month = Console.ReadLine();
      if (month != null && IsValidFormat(month))
        break;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the day directory name:");
      day = Console.ReadLine();
      if (day != null && IsValidFormat(day))
        break;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    return (year ?? "", month ?? "", day ?? "");
  }
  public static (string Year, string Day)? SetCustomDirectoryFormatByYD()
  {
    string? year = "", day = "";
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the year directory name:");
      var usingSpecifier = FormatSpecifier.Year | FormatSpecifier.Month | FormatSpecifier.Day;
      if (i == 0)
        OutputCustomFormatDescription(usingSpecifier);
      year = Console.ReadLine();
      if (year != null && IsValidFormat(year))
        break;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    for (var i = 0; i < 100; i++)
    {
      Console.WriteLine("Enter the custom format for the day directory name:");
      day = Console.ReadLine();
      if (day != null && IsValidFormat(day))
        break;
      else
        Console.Error.WriteLine("Invalid format.");
      if (i == 99)
      {
        OutputTryAgainMessage();
        return null;
      }
    }
    return (year ?? "", day ?? "");
  }

  private static void OutputCustomFormatDescription(FormatSpecifier usingSpecifier)
  {
    Console.WriteLine("You can use the following format specifiers:");
    if (usingSpecifier.HasFlag(FormatSpecifier.Year))
      Console.WriteLine("yyyy: Year like 2024");
    if (usingSpecifier.HasFlag(FormatSpecifier.Month))
      Console.WriteLine("MM: Month like 01");
    if (usingSpecifier.HasFlag(FormatSpecifier.Day))
      Console.WriteLine("dd: Day like 02");
    if (usingSpecifier.HasFlag(FormatSpecifier.Time))
    {
      Console.WriteLine("HH: Hour like 03 (24-hour clock)");
      Console.WriteLine("mm: Minute like 04");
      Console.WriteLine("ss: Second like 05");
    }
    if (usingSpecifier.HasFlag(FormatSpecifier.FileName))
      Console.WriteLine("NN: Original file name like IMG_0001 (Without extension. This specifier is only available for this application)");
    Console.WriteLine("If you want to use different format specifiers, you can check other format specifiers on this page: https://learn.microsoft.com/dotnet/standard/base-types/custom-date-and-time-format-strings (Not all format specifiers are supported)");
    if (usingSpecifier.HasFlag(FormatSpecifier.Time))
      Console.WriteLine("For example, if you want to use the format yyyyMMdd_HHmmss, enter yyyyMMdd_HHmmss. It will be converted to 20240102_030405.");
    else
      Console.WriteLine("For example, if you want to use the format yyyyMMdd, enter yyyyMMdd. It will be converted to 20240102.");
    Console.WriteLine("You cannot use the following characters: /, \\, :, *, ?, \", <, >, |");
  }
  private static void OutputTryAgainMessage() => Console.Error.WriteLine("You have tried too many times. Try again from the beginning.");

  private static bool IsValidFormat(string format)
  {
    var testDate = DateTimeOffset.Now;
    try
    {
      var stringed = testDate.ToString(format);
      if (stringed.Contains('/') || stringed.Contains('\\') || stringed.Contains(':') || stringed.Contains('*') || stringed.Contains('?') || stringed.Contains('"') || stringed.Contains('<') || stringed.Contains('>') || stringed.Contains('|'))
        return false;
      return true;
    }
    catch
    {
      return false;
    }
  }
}
