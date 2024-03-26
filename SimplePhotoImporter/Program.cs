using System;
using System.IO;
using System.Text.RegularExpressions;

Type? shellAppType = null;
dynamic? shell = null;


Console.WriteLine("Simple Photo Importer");

if (!OperatingSystem.IsWindows())
{
    Console.WriteLine("This program is only supported on Windows.");
    return;
}
shellAppType = Type.GetTypeFromProgID("Shell.Application");
if (shellAppType == null)
{
    Console.WriteLine("Failed to get Shell.Application COM object.");
    return;
}
shell = Activator.CreateInstance(shellAppType);

string fromPath;
while (true)
{
    Console.WriteLine("Enter the path to the folder you want to import from:");
    var inputFromPath = Console.ReadLine();

    if (inputFromPath == null || !Directory.Exists(inputFromPath))
        Console.WriteLine("The specified path does not exist.");
    else
    {
        fromPath = inputFromPath;
        break;
    }
}

string toPath;
while (true)
{
    Console.WriteLine("Enter the path to the folder you want to import to:");
    var inputToPath = Console.ReadLine();

    if (inputToPath == null || !Directory.Exists(inputToPath))
        Console.WriteLine("The specified path does not exist.");
    else
    {
        toPath = inputToPath;
        break;
    }
}
