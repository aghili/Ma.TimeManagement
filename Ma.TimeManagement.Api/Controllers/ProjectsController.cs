using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ma.TimeManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IAzureDevOpsService azureDevOpsService;

        public ProjectsController(IAzureDevOpsService azureDevOpsService)
        {
            this.azureDevOpsService = azureDevOpsService;
        }
        // GET: api/<WorkItemsController>
        [HttpGet]
        public async Task<IEnumerable<TeamProjectReferenceDto>> Get(CancellationToken cancellationToken)
        {
            return await azureDevOpsService.GetProjectsAsync(cancellationToken);
        }

        // GET api/<WorkItemsController>/5
        [HttpGet("{id}")]
        public async Task<TeamProjectReferenceDto> Get(Guid id,CancellationToken cancellationToken)
        {
            return await azureDevOpsService.GetProjectAsync(id,cancellationToken);
        }

        // POST api/<WorkItemsController>
        [HttpPost]
        public async Task<ActionResult<TeamProjectReferenceDto>> Post([FromBody] TeamProjectReferenceAddDto item,CancellationToken cancellationToken)
        {
            return Forbid();
        }

        // PUT api/<WorkItemsController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] TeamProjectReferenceAddDto item, CancellationToken cancellationToken)
        {
            return Forbid();
        }

        // DELETE api/<WorkItemsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            return Forbid();
        }
    }
}
