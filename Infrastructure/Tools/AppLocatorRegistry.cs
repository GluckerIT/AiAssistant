using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AiAssistant.Infrastructure.Tools;

public class AppLocatorRegistry : IAppLocator
{
    private static readonly string[] registryPaths =
    {
            @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
            @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };


    public Task<IReadOnlyList<AppEntry>> ListAsync(CancellationToken cancellationToken = default) {
        var result = new List<AppEntry> ();

        foreach (var keyPath in registryPaths)
        {
            var target = ".exe";
            cancellationToken.ThrowIfCancellationRequested();

            using var baseKey = Registry.LocalMachine.OpenSubKey(keyPath);
            if (baseKey is null) continue;

            foreach (var subKeyName in baseKey.GetSubKeyNames())
            {
                cancellationToken.ThrowIfCancellationRequested();


                using var appKey = baseKey.OpenSubKey(subKeyName);
                if (appKey is null) continue;

                var systemComponent = appKey.GetValue("SystemComponent");
                if (systemComponent is int value && value == 1) continue;

                var parentKeyName = appKey.GetValue("ParentKeyName");
                if (parentKeyName != null) continue;

                var releaseTypeKey = appKey.GetValue("ReleaseType") as string;
                if (releaseTypeKey == "Update" || releaseTypeKey == "Hotfix" || releaseTypeKey == "Security Update") continue;

                var name = appKey.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(name)) continue;

                var installLocation = appKey.GetValue("InstallLocation") as string ?? string.Empty;
                var displayIcon = appKey.GetValue("DisplayIcon") as string ?? string.Empty;

                int index = displayIcon.LastIndexOf(target, StringComparison.OrdinalIgnoreCase);
                if (index > 0)
                {
                    displayIcon = displayIcon.Substring(0, index + target.Length);
                }
                else if (index == -1) continue;


                result.Add(new AppEntry(name, installLocation, displayIcon, keyPath));
            }
        }

        return Task.FromResult<IReadOnlyList<AppEntry>>(result);


    }
}


