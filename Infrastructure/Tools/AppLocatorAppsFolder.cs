using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;

namespace AiAssistant.Infrastructure.Tools;

internal class AppLocatorAppsFolder : IAppLocator
{
    public Task<IReadOnlyList<AppEntry>> ListAsync (CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new List<AppEntry>();

        var packageManager = new PackageManager(); 
        var packages = packageManager.FindPackagesForUser(string.Empty);

      





        return Task.FromResult<IReadOnlyList<AppEntry>>(result);
    }
}
