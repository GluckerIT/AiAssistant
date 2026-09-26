using AiAssistant.Infrastructure.Tools;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;



//IAppLocator appLocator = new AppLocatorRegistry();
IAppLocator appLocator = new AppLocatorStartMenu();

var applications = await appLocator.ListAsync();
var index = 0;

foreach (var app in applications)
{
    Console.WriteLine($"Nomber: {index}");
    Console.WriteLine($"Name: {app.Name}");
    Console.WriteLine($"Path: {app.ExecutablePath}");
    Console.WriteLine($"Exe: {app.ExePath}");
    Console.WriteLine($"Source: {app.Source}");
    Console.WriteLine("----------------------------");
    index++;
}

Console.Write("Введите номер программы: ");
int number = int.Parse(Console.ReadLine());
try
{
    Process.Start(new ProcessStartInfo
    {
        FileName = applications[number].ExePath,
        UseShellExecute = true,
    });
}
catch(Win32Exception ex) when (ex.NativeErrorCode ==740)
{
    Process.Start(new ProcessStartInfo
    {
        FileName = applications[number].ExePath,
        UseShellExecute = true,
        Verb = "runas"
    });
}