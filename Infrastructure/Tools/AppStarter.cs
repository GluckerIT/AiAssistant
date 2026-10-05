using System;
using System.Collections.Generic;
using System.Text;
using AiAssistant.Infrastructure.Tools;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.ComponentModel.DataAnnotations;



namespace AiAssistant.Infrastructure.Tools;

public class AppStarter
{
    public async Task<IReadOnlyList<AppEntry>> GetAppListAsync()
    {
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
                var apps = await locator.ListAsync();
                listApps.AddRange(apps);
            }
            catch
            {
                continue;
            }
        }
        return listApps;
    }
}
