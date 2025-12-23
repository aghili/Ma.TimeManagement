using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ma.TimeManagement.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkItemsController : ControllerBase
    {
        private readonly IAzureDevOpsService azureDevOpsService;
        private readonly IUserService _userService;

        public WorkItemsController(IAzureDevOpsService azureDevOpsService, IUserService userService)
        {
            this.azureDevOpsService = azureDevOpsService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ICollection<WorkItemDto>> Get(CancellationToken cancellationToken)
        {
            return await azureDevOpsService.GetTasksAsync(cancellationToken);
        }

        // GET api/<WorkItemsController>/5
        [HttpGet("{id}")]
        public async Task<WorkItemDto> Get(int id,CancellationToken cancellationToken)
        {
            return await azureDevOpsService.GetWorkItemAsync(id,cancellationToken);
        }

        // POST api/<WorkItemsController>
        [HttpPost]
        public async Task<WorkItemDto> Post([FromBody] WorkItemAddDto item,CancellationToken cancellationToken)
        {
            return await azureDevOpsService.CreateWorkItemAsync(item.Title, item.State, item.OriginalEstimate ?? 0, item.RemainingWork ?? 0, item.CompletedWork ?? 0, item.WorkItemType, item.ProjectID, item.Discution,cancellationToken);
        }

        // PUT api/<WorkItemsController>/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] WorkItemAddDto item,CancellationToken cancellationToken)
        {
            var jsonPatch = new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument();
            await azureDevOpsService.UpdateWorkItemAsync(jsonPatch, id,cancellationToken);
        }

        // DELETE api/<WorkItemsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id,CancellationToken cancellationToken)
        {
            return Forbid();
        }
    }
}
