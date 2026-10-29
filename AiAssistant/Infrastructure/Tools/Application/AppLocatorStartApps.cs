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
using System.Runtime.InteropServices;

namespace AiAssistant.Infrastructure.Tools.Application;

public class AppLocatorStartApps : IAppLocator
{
    public class StartAppsEntry
    {
        public string Name { get; set; } = string.Empty;
        public string AppID { get; set; } = string.Empty;
    }

    public async Task<IReadOnlyList<AppEntry>> ListAsync (CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> replacementAUMIDPath = new()
        {
            {"{6D809377-6AF0-444B-8957-A3773F02200E}", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)},
            {"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)},
            {"{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}", Environment.GetFolderPath(Environment.SpecialFolder.System)},
            {"{D65231B0-B2F1-4857-A4CE-A8E7C6EA7D27}", Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)},
            {"{5CD7AEE2-2219-4A67-B85D-6C9CE15660CB}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)},
        };

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
            var appIDPath = app.AppID;
            foreach (var replacement in replacementAUMIDPath)
            {
                if(appIDPath.StartsWith(replacement.Key, StringComparison.OrdinalIgnoreCase))
                {
                    appIDPath = replacement.Value + appIDPath.Substring(replacement.Key.Length);
                    break;
                }

            }
            if(File.Exists(appIDPath))
            {
                result.Add(new AppEntry(app.Name, "", appIDPath, "StartApps"));
            }
            else
            {
                result.Add(new AppEntry(app.Name,appIDPath, "",  "StartApps"));

            }

        }
        return result;
    }
}
