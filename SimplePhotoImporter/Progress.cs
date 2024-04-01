namespace SimplePhotoImporter;

#pragma warning disable CS0162

public static class AssemblyState
{
  public const bool IsDebug =
#if DEBUG
    true;
#else
    false;
#endif
}

public class ProgressBar
{
  private readonly int width;
  private readonly int total;
  private int current = 0;
  private int textCursorTop;
  bool isWroteText = false;
  private readonly int maxTextLength;

  public ProgressBar(int width, int total)
  {
    this.width = width;
    this.total = total;
    previousCursorTop = Console.CursorTop;
    textCursorTop = -1;
    // "#[" + "<"*width + "]#" + "(100.0%<-100/100)"
    maxTextLength = width + 4 + 11 + total.ToString().Length * 2;

    Console.CursorVisible = false;

    Update(0);
  }

  public void Update()
  {
    Update(++current);
  }
  public void Update(int current)
  {
    Update(current, string.Empty);
  }
  public void Update(string text)
  {
    Update(++current, text);
  }

  int previousCursorTop;
  public void Update(int current, string text)
  {
    this.current = current;

    float parcent = (float)current / total;
    int widthNow = (int)Math.Floor(width * parcent);

    string gauge = new string('>', widthNow) + new string(' ', width - widthNow);
    string status = $"({parcent * 100:f1}%<-{current}/{total})";

    Console.SetCursorPosition(0, previousCursorTop);
    Console.WriteLine($"#[{gauge}]#{status}".PadRight(maxTextLength, ' '));

    //if (!AssemblyState.IsDebug)
    previousCursorTop = Console.CursorTop - 1;

    if (!string.IsNullOrEmpty(text))
      WriteText(text);
  }

  public void Done()
  {
    const string MESSAGE = "Done";

    int sideLen = (int)Math.Floor((float)(width - MESSAGE.Length) / 2);
    string gauge = new string('=', sideLen) + MESSAGE;
    gauge += new string('=', width - gauge.Length);
    string status = $"(100.0%<-{total}/{total})";

    Console.SetCursorPosition(0, previousCursorTop);
    Console.WriteLine($"#[{gauge}]#{status} ".PadRight(maxTextLength, ' '));

    if (AssemblyState.IsDebug)
    {
      if (textCursorTop == -1)
        textCursorTop = Console.CursorTop;
      Console.SetCursorPosition(0, textCursorTop);
    }
    else
    {
      if (isWroteText)
      {
        Console.SetCursorPosition(0, textCursorTop);
        Console.WriteLine();
      }
    }

    Console.CursorVisible = true;
  }

  public void WriteText(string text)
  {
    if (AssemblyState.IsDebug)
    {
      if (textCursorTop == -1)
      {
        textCursorTop = Console.CursorTop;
      }
      Console.SetCursorPosition(0, textCursorTop);
      if (isWroteText)
        Console.WriteLine();
      Console.Write(text);
      if (isWroteText)
        previousCursorTop--;
      isWroteText = true;
    }
    else
    {
      if (textCursorTop != -1)
      {
        isWroteText = true;
        Console.SetCursorPosition(0, textCursorTop);
        Console.WriteLine();
      }
      Console.Write(text);
      textCursorTop = Console.CursorTop;
      if (isWroteText)
      {
        previousCursorTop--;
      }
    }
  }
}
