using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AiAssistant.Infrastructure.Tools.Application;

namespace ASPAiAssistant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly AppStarterManager _appStarterManager;

        public ApplicationController(AppStarterManager appStarterManager)
        {
            _appStarterManager = appStarterManager;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetApplicationList()
        {
            var appList = await _appStarterManager.GetAppListAsync();
            var deduplicatedList = _appStarterManager.DeduplicationAndMergeList(appList);
            return Ok(deduplicatedList);
        }

        [HttpPost("launch/{id}")]
        public async Task<IActionResult> LaunchApplication(int id)
        {
            var appList = await _appStarterManager.GetAppListAsync();
            var deduplicatedList = _appStarterManager.DeduplicationAndMergeList(appList);
            var app = deduplicatedList.FirstOrDefault(a => a.Id == id);
            if (app == null)
            {
                return NotFound();
            }
            await Task.Run(() => _appStarterManager.LaunchApplication(app));
            return Ok();
        }

    }
}
