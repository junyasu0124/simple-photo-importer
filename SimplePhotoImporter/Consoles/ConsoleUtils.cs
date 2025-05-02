using System.Runtime.InteropServices;

namespace SimplePhotoImporter.Consoles;

public static class ConsoleUtils
{
  private static readonly uint STD_OUTPUT_HANDLE = unchecked((uint)-11);
  private static readonly uint STD_ERROR_HANDLE = unchecked((uint)-12);
  private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);
  private static readonly uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

  [DllImport("kernel32.dll")]
  private extern static IntPtr GetStdHandle(uint nStdHandle);

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private extern static bool GetConsoleMode(IntPtr hConsoleHandle, out uint mode);

  [DllImport("kernel32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private extern static bool SetConsoleMode(IntPtr hConsoleHandle, uint mode);

  public static void InitializeConsole()
  {
    IntPtr consoleOutputHandle;
    if (!Console.IsOutputRedirected)
      consoleOutputHandle = GetStdHandle(STD_OUTPUT_HANDLE);
    else if (!Console.IsErrorRedirected)
      consoleOutputHandle = GetStdHandle(STD_ERROR_HANDLE);
    else
      consoleOutputHandle = INVALID_HANDLE_VALUE;

    if (consoleOutputHandle != INVALID_HANDLE_VALUE)
    {
      if (!GetConsoleMode(consoleOutputHandle, out var mode))
        throw new Exception("Failed to get console mode.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

      if ((mode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == 0)
      {
        if (!SetConsoleMode(consoleOutputHandle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING))
          throw new Exception("Failed to set console mode.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
      }
    }
  }

  public static void Print() => Console.WriteLine();
  public static void Print(string message) => Console.WriteLine(message);
  public static void PrintError(string message) => Console.Error.WriteLine($"\x1b[91m{message}\x1b[39m");
}
