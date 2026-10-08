using System;
using System.Collections.Generic;
using System.Text;
using AiAssistant.Infrastructure.Tools;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using Windows.ApplicationModel.Activation;

namespace AiAssistant.Infrastructure.Tools;

public class AppStarterManager
{
    public sealed record Application(
        string Name  ,
        string ShellStart,
        string ExePath);

    public async Task<IReadOnlyList<AppEntry>> GetAppListAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"сбор списка приложений");

        cancellationToken.ThrowIfCancellationRequested();
        //IAppLocator registryLocator = new AppLocatorRegistry();
        IAppLocator startMenuLocator = new AppLocatorStartMenu();
        IAppLocator startAppsLocator = new AppLocatorStartApps();

        var locators = new List<IAppLocator>
        {
            //registryLocator,
            startMenuLocator,
            startAppsLocator
        };

        var listApps = new List<AppEntry>();
        foreach (var locator in locators)
        {
            try
            {
                var apps = await locator.ListAsync(cancellationToken);
                listApps.AddRange(apps);
            }
            catch(OperationCanceledException)
            {
                throw;
            }
            catch
            {
                continue;
            }
        }
        return listApps.OrderBy(a => a.Name).ToList();
    }

    public IReadOnlyList<Application> DeduplicationList(IReadOnlyList<AppEntry> apps)
    {
        var result = new List<Application>();
        var tempExe = new List<Application>();
        var tempShellStart = new List<Application>();

        foreach (var app in apps) {
            if (!string.IsNullOrEmpty(app.ExecutablePath))
            {
                if (!tempExe.Any(a => string.Equals(a.ExePath, app.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                {
                    tempExe.Add(new Application(app.Name, "", app.ExecutablePath));
                }
            }
            if (!string.IsNullOrEmpty(app.InstallFolder))
            {
                if (!tempShellStart.Any(a => string.Equals(a.ShellStart, app.InstallFolder, StringComparison.OrdinalIgnoreCase)))
                {
                    tempShellStart.Add(new Application(app.Name, app.InstallFolder, ""));
                }
            }
        }
        List<int> deleteNumberExe = new List<int>();
        List<int> deleteNumberShellStart = new List<int>();
        foreach (var exeApp in tempExe)
        {
            foreach(var shellApp in tempShellStart)
            {
                if(!string.IsNullOrEmpty(exeApp.ShellStart) && 
                   !string.IsNullOrEmpty(shellApp.ShellStart) && 
                   !string.Equals(exeApp.ShellStart, shellApp.ShellStart, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(exeApp.ExePath) && 
                    !string.IsNullOrEmpty(shellApp.ExePath) &&
                    !string.Equals(exeApp.ExePath, shellApp.ExePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (string.Equals(exeApp.Name, shellApp.Name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new Application(exeApp.Name, shellApp.ShellStart, exeApp.ExePath));
                    deleteNumberExe.Add(tempExe.IndexOf(exeApp));
                    deleteNumberShellStart.Add(tempShellStart.IndexOf(shellApp));                   
                }
            }
        }
        for (int i = deleteNumberExe.Count - 1; i >= 0; i--)
        {
            tempExe.RemoveAt(deleteNumberExe[i]);
        }
        for (int i = deleteNumberShellStart.Count - 1; i >= 0; i--)
        {
            tempShellStart.RemoveAt(deleteNumberShellStart[i]);
        }
        result.AddRange(tempExe);
        result.AddRange(tempShellStart);

        return result.OrderBy(a => a.Name).ToList();
    }

    public async Task StartProgram()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var applications = await GetAppListAsync(cancellationTokenSource.Token);
        var uniqueApplications = DeduplicationList(applications);
        var index = 0;



        foreach (var app in uniqueApplications)
        {
            Console.WriteLine($"Nomber: {index}");
            Console.WriteLine($"Name: {app.Name}");
            Console.WriteLine($"Launch not exe: {app.ShellStart}");
            Console.WriteLine($"Exe: {app.ExePath}");
            Console.WriteLine("----------------------------");
            index++;
        }

        //foreach (var app in applications)
        //{
        //    Console.WriteLine($"Nomber: {index}");
        //    Console.WriteLine($"Name: {app.Name}");
        //    Console.WriteLine($"Launch not exe: {app.InstallFolder}");
        //    Console.WriteLine($"Exe: {app.ExecutablePath}");
        //    Console.WriteLine($"Source: {app.Source}");
        //    Console.WriteLine("----------------------------");
        //    index++;
        //}

        //Console.Write("Введите номер программы: ");
        //int number = int.Parse(Console.ReadLine());
        //try
        //{
        //    Process.Start(new ProcessStartInfo
        //    {
        //        FileName = applications[number].ExecutablePath,
        //        UseShellExecute = true,
        //    });
        //}
        //catch (Win32Exception ex) when (ex.NativeErrorCode == 740)
        //{
        //    Process.Start(new ProcessStartInfo
        //    {
        //        FileName = applications[number].ExecutablePath,
        //        UseShellExecute = true,
        //        Verb = "runas"
        //    });
        //}
    }

}
