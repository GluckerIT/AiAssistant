using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using Windows.ApplicationModel.Activation;

namespace AiAssistant.Infrastructure.Tools.Application;

public class AppStarterManager
{
    public sealed record AppLaunch(
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

    public IReadOnlyList<AppLaunch> DeduplicationAndMergeList(IReadOnlyList<AppEntry> apps)
    {
        var result = new List<AppLaunch>();
        var tempExe = new List<AppLaunch>();
        var tempShellStart = new List<AppLaunch>();

        foreach (var app in apps) {
            if (!string.IsNullOrEmpty(app.ExecutablePath))
            {
                if (!tempExe.Any(a => string.Equals(a.ExePath, app.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                {
                    tempExe.Add(new AppLaunch(app.Name, "", app.ExecutablePath));
                }
            }
            if (!string.IsNullOrEmpty(app.InstallFolder))
            {
                if (!tempShellStart.Any(a => string.Equals(a.ShellStart, app.InstallFolder, StringComparison.OrdinalIgnoreCase)))
                {
                    tempShellStart.Add(new AppLaunch(app.Name, app.InstallFolder, ""));
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
                    result.Add(new AppLaunch(exeApp.Name, shellApp.ShellStart, exeApp.ExePath));
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

    public enum LaunchResult
    {
        Success,
        Failed,
        Cancelled
    }

    public static LaunchResult TryStart (string fileName, string? arguments = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = true
        };
        if(!string.IsNullOrEmpty(arguments))
        {
            startInfo.Arguments = arguments;
        }
        try
        {
            Process.Start(startInfo);
            return LaunchResult.Success;
        }
        catch (Win32Exception ex2) when (ex2.NativeErrorCode == 1223)
        {
            Console.WriteLine(ex2.NativeErrorCode);
            return LaunchResult.Cancelled;
        }
        catch (Win32Exception)
        {
            return LaunchResult.Failed;
        }
    }

    public void LaunchApplication(AppLaunch app, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrEmpty(app.ExePath) && File.Exists(app.ExePath))
        {
            var result = TryStart(app.ExePath);
            if (result == LaunchResult.Success)
            {
                Console.WriteLine($"Приложение {app.Name} успешно запущено.");
                return;
            }
            else if (result == LaunchResult.Cancelled)
            {
                Console.WriteLine($"Запуск приложения {app.Name} был отменен пользователем.");
                return;
            }
        }
        if (!string.IsNullOrEmpty(app.ShellStart))
        {
            var result = TryStart("explorer.exe", $@"shell:AppsFolder\{app.ShellStart}");
            if (result == LaunchResult.Success)
            {
                Console.WriteLine($"Приложение {app.Name} успешно запущено.");
                return;
            }
            else if (result == LaunchResult.Cancelled)
            {
                Console.WriteLine($"Запуск приложения {app.Name} был отменен пользователем.");
                return;
            }
            else if (result == LaunchResult.Failed)
            {
                Console.WriteLine($"Не удалось запустить приложение {app.Name}.");
                return;
            }

        }
        Console.WriteLine($"Не удалось запустить приложение {app.Name}.");
        return;
    }
    
    public async Task StartProgram()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var applications = await GetAppListAsync(cancellationTokenSource.Token);
        var uniqueApplications = DeduplicationAndMergeList(applications);
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
        Console.Write("Введите номер программы: ");
        int number;
        while(!int.TryParse(Console.ReadLine(), out number))
        {
            Console.Write("Введите корректный номер программы: ");
        }
        if (number >= 0 && number < uniqueApplications.Count)
        {
            LaunchApplication(uniqueApplications[number], cancellationTokenSource.Token);
        }
        else
        {
            Console.WriteLine("Неверный номер программы.");
        }
    }

}
