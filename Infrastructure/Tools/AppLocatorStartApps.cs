using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace AiAssistant.Infrastructure.Tools;

internal class AppLocatorStartApps : IAppLocator
{
    public class StartAppsEntry
    {
        public string Name { get; set; } = string.Empty;
        public string AppID { get; set; } = string.Empty;
    }

    public async Task<IReadOnlyList<AppEntry>> ListAsync (CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new List<AppEntry>();

        var startShell = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            Arguments = "-NoProfile -NonInteractive -Command \"Get-StartApps | ConvertTo-Json -Compress\""
        };

        using var process = Process.Start(startShell);

        if (process == null)
        {
            throw new InvalidOperationException("Failed to start PowerShell process.");
        }

        string json = await process.StandardOutput.ReadToEndAsync(cancellationToken);


        await process.WaitForExitAsync(cancellationToken);

        var startApps = JsonSerializer.Deserialize<List<StartAppsEntry>>(json) ?? new List<StartAppsEntry>();

        foreach (var app in startApps)
        {
            result.Add(new AppEntry (app.Name, app.AppID, app.AppID, "StartApps"));
        }
        return result;
    }
}
