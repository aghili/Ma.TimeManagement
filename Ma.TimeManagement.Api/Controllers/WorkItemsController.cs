using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IConverterService converterService;

        public WorkItemsController(IAzureDevOpsService azureDevOpsService, IUserService userService,IConverterService converterService)
        {
            this.azureDevOpsService = azureDevOpsService;
            _userService = userService;
            this.converterService = converterService;
        }

        [HttpGet]
        public async Task<ICollection<WorkItemDto>> Get(CancellationToken cancellationToken)
        {
            return await azureDevOpsService.GetTasksAsync(cancellationToken);
        }

        // GET api/<WorkItemsController>/5
        [HttpGet("{id}")]
        public async Task<WorkItemDto?> Get(int id,CancellationToken cancellationToken)
        {
            return await azureDevOpsService.GetWorkItemAsync(id,cancellationToken);
        }

        // POST api/<WorkItemsController>
        [HttpPost]
        public async Task<WorkItemDto> Post(WorkItemAddDto item, CancellationToken cancellationToken)
        {
            return await azureDevOpsService.CreateWorkItemAsync(item, cancellationToken);
        }

        // PUT api/<WorkItemsController>/5
        [HttpPut("{id}")]
        public async Task Put(int id, WorkItemAddDto workItem,CancellationToken cancellationToken)
        {
            //var jsonPatch = new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument();
            await azureDevOpsService.UpdateWorkItemAsync(id,workItem,cancellationToken);
        }
        // PUT api/<WorkItemsController>/5
        [HttpPatch("{id}")]
        public async Task Patch(int id, WorkItemUpdateDto workItem, CancellationToken cancellationToken)
        {
            var jsonPatch = new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument();
            await azureDevOpsService.UpdateWorkItemAsync(id, workItem, cancellationToken);
        }

        // DELETE api/<WorkItemsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id,CancellationToken cancellationToken)
        {
            return Forbid();
        }
    }
}
