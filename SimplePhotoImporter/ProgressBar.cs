namespace SimplePhotoImporter;

public class ProgressBar
{
  private readonly int width;
  private readonly int total;
  private int current;
  private int barCursorTop = -1;
  private readonly int maxTextLength;

  public ProgressBar(int width, int total)
  {
    this.width = width;
    this.total = total;

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
    this.current = current;

    float parcent = (float)current / total;
    int widthNow = (int)Math.Floor(width * parcent);

    string gauge = new string('>', widthNow) + new string(' ', width - widthNow);
    string status = $"({parcent * 100:f1}%<-{current}/{total})";

    Console.SetWindowPosition(0, 0);

    if (Console.CursorTop < Console.WindowHeight - 1)
    {
      barCursorTop = Console.CursorTop;
    }
    else
    {
      barCursorTop = Console.WindowHeight - 1;
    }
    Console.SetCursorPosition(0, barCursorTop);
    Console.Write($"#[{gauge}]#{status}".PadRight(maxTextLength, ' '));
  }

  public void Done()
  {
    const string MESSAGE = "Done";

    int sideLen = (int)Math.Floor((float)(width - MESSAGE.Length) / 2);
    string gauge = new string('=', sideLen) + MESSAGE;
    gauge += new string('=', width - gauge.Length);
    string status = $"(100.0%<-{total}/{total})";

    Console.SetCursorPosition(0, barCursorTop);
    Console.WriteLine($"#[{gauge}]#{status} ".PadRight(maxTextLength, ' '));

    Console.CursorVisible = true;
  }
}
