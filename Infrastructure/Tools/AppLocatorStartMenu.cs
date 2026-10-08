using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiAssistant.Infrastructure.Tools;

public class AppLocatorStartMenu : IAppLocator
{
    public Task<IReadOnlyList<AppEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<AppEntry>();

        var shellType = Type.GetTypeFromProgID("WScript.Shell");
        dynamic shell = Activator.CreateInstance(shellType)!;

        if (shellType is null) return Task.FromResult<IReadOnlyList<AppEntry>>(result);

        string[] startMenuFolder = {
            Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
            Environment.GetFolderPath(Environment.SpecialFolder.Programs)
        };

        foreach(var folder in startMenuFolder)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(folder)) continue;

            var shortcutFiles = Directory.EnumerateFiles(folder, "*.lnk", SearchOption.AllDirectories);

            foreach(var shortcutPath in shortcutFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    dynamic shortcut = shell.CreateShortcut(shortcutPath);
                    string targetPath = shortcut.TargetPath;

                    if(string.IsNullOrWhiteSpace(targetPath)) continue;
                    if(!targetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;
                    if(!File.Exists(targetPath)) continue;

                    var name = Path.GetFileNameWithoutExtension(shortcutPath);
                    result.Add(new AppEntry(name, "", targetPath, "Start Menu"));
                }
                catch
                {
                    continue;
                }
            }
        }
        return Task.FromResult<IReadOnlyList<AppEntry>>(result);
    }
}
