using System.Text.RegularExpressions;

namespace SimplePhotoImporter;

public static partial class CustomFormatter
{
  private static (int Start, int End)[] CountQuots(string format)
  {
    List<(int Start, int End)> result = [];
    char? target = null;
    int targetStart = -1;
    for (int i = 0; i < format.Length; i++)
    {
      if (target == '\"')
      {
        if (format[i] == '\"')
        {
          result.Add((targetStart, i));
          target = null;
        }
      }
      else if (target == '\'')
      {
        if (format[i] == '\'')
        {
          result.Add((targetStart, i));
          target = null;
        }
      }
      else
      {
        if (format[i] == '\"')
        {
          if (i != 0 && format[i - 1] == '\\')
            continue;
          target = '\"';
          targetStart = i;
        }
        else if (format[i] == '\'')
        {
          if (i != 0 && format[i - 1] == '\\')
            continue;
          target = '\'';
          targetStart = i;
        }
      }
    }
    return [.. result];
  }
  private static string ReplaceSpecifiers(string format, Func<Regex> formatSpecifier, string replacement)
  {
    var matches = formatSpecifier().Matches(format).Select(match => (match.Index, match.Length)).ToArray();
    foreach (var (start, length) in matches)
    {
      if (CountQuots(format).Any(range => range.Start <= start && start + length <= range.End))
        continue;
      format = format.Remove(start, length).Insert(start, replacement);
    }
    return format;
  }
  private static string Unescape(string format)
  {
    var replaced = EscapeCharacter().Replace(format, "$1");
    var matches = CountQuots(replaced);
    for (int i = matches.Length - 1; i >= 0; i--)
    {
      var (start, end) = matches[i];
      replaced = replaced.Remove(end, 1).Remove(start, 1);
    }
    return replaced;
  }
  [GeneratedRegex(@"\\(.)")]
  private static partial Regex EscapeCharacter();

  public static string ToCustomYear(this string format, int year)
  {
    format = ReplaceSpecifiers(format, yFormatSpecifier, int.Parse(year.ToString("00000")[^2..]).ToString());

    format = ReplaceSpecifiers(format, yyFormatSpecifier, year.ToString("00000")[^2..]);

    format = ReplaceSpecifiers(format, yyyFormatSpecifier, year.ToString("00000")[^3..]);

    format = ReplaceSpecifiers(format, yyyyFormatSpecifier, year.ToString("00000")[^4..]);

    format = ReplaceSpecifiers(format, yyyyyFormatSpecifier, year.ToString("00000")[^5..]);

    return Unescape(format);
  }

  [GeneratedRegex("""(?<![y\\])y(?!y)""")]
  private static partial Regex yFormatSpecifier();

  [GeneratedRegex("""(?<![y\\])yy(?!y)""")]
  private static partial Regex yyFormatSpecifier();

  [GeneratedRegex("""(?<![y\\])yyy(?!y)""")]
  private static partial Regex yyyFormatSpecifier();

  [GeneratedRegex("""(?<![y\\])yyyy(?!y)""")]
  private static partial Regex yyyyFormatSpecifier();

  [GeneratedRegex("""(?<![y\\])yyyyy(?!y)""")]
  private static partial Regex yyyyyFormatSpecifier();


  public static string ToCustomMonth(this string format, int month)
  {
    format = ReplaceSpecifiers(format, MFormatSpecifier, int.Parse(month.ToString("00000")[^2..]).ToString());

    format = ReplaceSpecifiers(format, MMFormatSpecifier, month.ToString("00000")[^2..]);

    format = ReplaceSpecifiers(format, MMMFormatSpecifier, new DateOnly(1, month, 1).ToString("MMM"));

    format = ReplaceSpecifiers(format, MMMMFormatSpecifier, new DateOnly(1, month, 1).ToString("MMMM"));

    return Unescape(format);
  }

  [GeneratedRegex("""(?<![M\\])M(?!M)""")]
  private static partial Regex MFormatSpecifier();

  [GeneratedRegex("""(?<![M\\])MM(?!M)""")]
  private static partial Regex MMFormatSpecifier();

  [GeneratedRegex("""(?<![M\\])MMM(?!M)""")]
  private static partial Regex MMMFormatSpecifier();

  [GeneratedRegex("""(?<![M\\])MMMM(?!M)""")]
  private static partial Regex MMMMFormatSpecifier();


  public static string ToCustomDay(this string format, int day)
  {
    format = ReplaceSpecifiers(format, dFormatSpecifier, int.Parse(day.ToString("00000")[^2..]).ToString());

    format = ReplaceSpecifiers(format, ddFormatSpecifier, day.ToString("00000")[^2..]);

    format = ReplaceSpecifiers(format, dddFormatSpecifier, new DateOnly(1, 1, day).ToString("ddd"));

    format = ReplaceSpecifiers(format, ddddFormatSpecifier, new DateOnly(1, 1, day).ToString("dddd"));
    return Unescape(format);
  }

  [GeneratedRegex("""(?<![d\\])d(?!d)""")]
  private static partial Regex dFormatSpecifier();

  [GeneratedRegex("""(?<![d\\])dd(?!d)""")]
  private static partial Regex ddFormatSpecifier();

  [GeneratedRegex("""(?<![d\\])ddd(?!d)""")]
  private static partial Regex dddFormatSpecifier();

  [GeneratedRegex("""(?<![d\\])dddd(?!d)""")]
  private static partial Regex ddddFormatSpecifier();


  /// <summary>
  /// Replace the file name format specifieres "NN" with the file name without extension.
  /// </summary>
  /// <returns>Replaced file name without extension.</returns>
  public static string ToCustomFileName(this string format, string fileName)
  {
    return ReplaceSpecifiers(format, fileNameFormatSpecifier, Path.GetFileNameWithoutExtension(fileName));
  }

  [GeneratedRegex("""(?<![N\\])NN(?!N)""")]
  private static partial Regex fileNameFormatSpecifier();
}
