using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ma.TimeManagement.Services
{
    public class AzureDevOpsService
    {
        public static AzureDevOpsService Instance { get; } = new AzureDevOpsService();

        public WorkItemTrackingHttpClient WitClient { get; private set; }
        public string Project { get; private set; }

        private AzureDevOpsService() { }

        public void Initialize(Uri uri, VssCredentials credentials, string project)
        {
            var connection = new VssConnection(uri, credentials);
            WitClient = connection.GetClient<WorkItemTrackingHttpClient>();
            Project = project;
        }
    }
}
