namespace SimplePhotoImporter;

public class Progress(int width, int total)
{
  public int columns = Console.WindowWidth;
  public int width = width;
  public int current = 0;
  public int total = total;
  private int rowLate = Console.CursorTop;

  public void Update(string message)
  {
    int row0 = Console.CursorTop;

    float parcent = (float)current / total;
    int widthNow = (int)Math.Floor(width * parcent);

    string gauge = new string('>', widthNow) + new string(' ', width - widthNow);
    string status = $"({parcent * 100:f1}%<-{current}/{total})";

    Console.WriteLine($"#[{gauge}]#{status}");
    ClearScreenDown();

    Console.WriteLine(message);
    rowLate = Console.CursorTop;
    Console.SetCursorPosition(0, row0);
    current++;
  }

  public void Done(string doneAlert)
  {
    int sideLen = (int)Math.Floor((float)(width - doneAlert.Length) / 2);

    string gauge = new string('=', sideLen) + doneAlert;
    gauge += new string('=', width - gauge.Length);
    string status = $"(100%<-{total}/{total})";

    Console.WriteLine($"#[{gauge}]#{status}");
  }

  private void ClearScreenDown()
  {
    int clearRange = rowLate - (Console.CursorTop - 1);
    Console.Write(new string(' ', columns * clearRange));
    Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - clearRange);
  }
}
