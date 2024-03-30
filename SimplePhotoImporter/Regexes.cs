using System.Text.RegularExpressions;

namespace SimplePhotoImporter;
public static partial class CustomRegexes
{

  [GeneratedRegex("""[^1-5 ]""")]
  public static partial Regex MatchNotWayToGetShootingDateTime();


  [GeneratedRegex(@"[^0-9]")]
  public static partial Regex MatchOtherThanNumbers();


  [GeneratedRegex(@"(?<!(?<!\\)\\)"".*?""|[^ ]+")]
  public static partial Regex MatchInputPahts();
}
