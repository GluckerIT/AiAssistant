using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AiAssistant.Infrastructure.Tools;

public class AppLocatorStartMenu: IAppLocator
{
    private static readonly string[] startMenuPath =
    {
        @"%ProgramData%\Microsoft\Windows\Start Menu\Programs",
        @"%AppData%\Microsoft\Windows\Start Menu\Programs"
    };

    public Task<IReadOnlyList<AppEntry>> ListAsync (CancellationToken cancellationToken = default)
    {
        
        return ListAsync (cancellationToken);
    }
}