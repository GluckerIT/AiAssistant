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



namespace AiAssistant.Infrastructure.Tools;

public class AppStarterManager
{
    public sealed record AppStartPriority(
        string Name,
        string Source);

    public async Task<IReadOnlyList<AppEntry>> GetAppListAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"сбор списка приложений");

        cancellationToken.ThrowIfCancellationRequested();
        IAppLocator registryLocator = new AppLocatorRegistry();
        IAppLocator startMenuLocator = new AppLocatorStartMenu();
        IAppLocator startAppsLocator = new AppLocatorStartApps();

        var locators = new List<IAppLocator>
        {
            registryLocator,
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
        return listApps;
    }

    public IReadOnlyList<AppEntry> SortByName(IReadOnlyList<AppEntry> apps)
    {
        Console.WriteLine($"сортировка списка приложений");
        return apps.OrderBy(app => app.Name).ToList();
    }

    public IReadOnlyList<AppEntry> DeduplicationList(IReadOnlyList <AppEntry> apps)
    {
        var result = new List<AppEntry>();




        return result;
    }

    public async Task StartProgram()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var listApps = await GetAppListAsync(cancellationTokenSource.Token);
        var applications = SortByName(listApps);
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
        catch (Win32Exception ex) when (ex.NativeErrorCode == 740)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = applications[number].ExecutablePath,
                UseShellExecute = true,
                Verb = "runas"
            });
        }
    }

}
