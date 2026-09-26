using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AiAssistant.Infrastructure.Tools;

public sealed record AppEntry(string Name, string InstallFolder, string ExecutablePath, string Source);

public interface IAppLocator
{

    Task<IReadOnlyList<AppEntry>> ListAsync(CancellationToken cancellationToken = default);
}

