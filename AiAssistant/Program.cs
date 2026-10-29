using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using AiAssistant.Infrastructure.Tools.Application;
using AiAssistant.Infrastructure.Tools.Notes;
using System.Threading.Tasks;


AppStarterManager starter = new AppStarterManager();
NotesRepository createNotes = new NotesRepository();

createNotes.Main();
//await starter.StartProgram();

