using AiAssistant.Infrastructure.Tools;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;







//IAppLocator appLocator = new AppLocatorRegistry();
//IAppLocator appLocator = new AppLocatorStartMenu();
//IAppLocator appLocator = new AppLocatorStartApps();

//var finalList = new List<AppEntry>();


AppStarter starter = new AppStarter();
starter.GetAppListAsync().Wait();
var applications = await starter.GetAppListAsync();

var index = 0;

foreach (var app in applications)
{
    Console.WriteLine($"Nomber: {index}");
    Console.WriteLine($"Name: {app.Name}");
    Console.WriteLine($"Path: {app.InstallFolder}");
    Console.WriteLine($"Exe: {app.ExecutablePath}");
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
        FileName = applications[number].ExecutablePath,
        UseShellExecute = true,
    });
}
catch(Win32Exception ex) when (ex.NativeErrorCode ==740)
{
    Process.Start(new ProcessStartInfo
    {
        FileName = applications[number].ExecutablePath,
        UseShellExecute = true,
        Verb = "runas"
    });
}