namespace SimplePhotoImporter;

public class ProgressBar
{
  private readonly int width;
  private readonly int total;
  private int current = 0;
  private readonly int firstCursorTop;
  private int textCursorTop;
  private readonly int maxTextLength;

  public ProgressBar(int width, int total)
  {
    this.width = width;
    this.total = total;
    firstCursorTop = Console.CursorTop;
    textCursorTop = firstCursorTop + 1;
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

  public void Update(int current, string text)
  {
    this.current = current;

    float parcent = (float)current / total;
    int widthNow = (int)Math.Floor(width * parcent);

    string gauge = new string('>', widthNow) + new string(' ', width - widthNow);
    string status = $"({parcent * 100:f1}%<-{current}/{total})";

    Console.SetCursorPosition(0, firstCursorTop);
    Console.WriteLine($"#[{gauge}]#{status}".PadRight(maxTextLength, ' '));
    //cursorTop--;

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

    Console.SetCursorPosition(0, firstCursorTop);
    Console.WriteLine($"#[{gauge}]#{status} ".PadRight(maxTextLength, ' '));

    Console.SetCursorPosition(0, textCursorTop);
    Console.CursorVisible = true;
  }

  public void WriteText(string text)
  {
    Console.SetCursorPosition(0, textCursorTop++);
    Console.WriteLine(text);
  }
}
