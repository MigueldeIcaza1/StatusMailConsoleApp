using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interfaces;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Enums;
using Models;

namespace TfsProject
{
    public class TfsService : ITfsService
    {
        private WorkItemTrackingHttpClient witClient;

        private TswaClientHyperlinkService hyperLinkService;

        public async Task<List<StatusRecord>> GetDataFromTFS(Uri collectionUri, string projectName, List<string> queryFolderHierarchy)
        {

            QueryHierarchyItem query = await GetQueryFromTFS(collectionUri, projectName, queryFolderHierarchy);
            Console.WriteLine("Retrieved the Query");

            WorkItemQueryResult queryResult = this.witClient.QueryByIdAsync(query.Id).Result;
            Console.WriteLine("Executed the Query");

            List<StatusRecord> statusRecords = await MapQueryResultToStatusRecords(queryResult);

            PrintStatusRecords(statusRecords);

            return statusRecords;
        }
       
        private async Task<QueryHierarchyItem> GetQueryFromTFS(Uri collectionUri, string projectName, List<string> queryFolderHierarchy)
         {
            witClient = await ConnectToTFS(collectionUri);

            Console.WriteLine("Connected to TFS");
            
            List<QueryHierarchyItem> queryHierarchyItems = this.witClient.GetQueriesAsync(projectName, depth: 2).Result;

            // QueryHierarchyItem queryHierarchyItem = queryHierarchyItems.FirstOrDefault(qhi => qhi.Name.Equals("Shared Queries"));

            QueryHierarchyItem queryHierarchyItem = queryHierarchyItems.FirstOrDefault();

            for (int index = 0; index < queryFolderHierarchy.Count; index++)
            {
                if (index == 0)
                {
                    queryHierarchyItem = queryHierarchyItems.FirstOrDefault(qhi => qhi.Name.Equals(queryFolderHierarchy[index]));
                }
                else
                {
                    queryHierarchyItem = queryHierarchyItem.Children.FirstOrDefault(qhi => qhi.Name.Equals(queryFolderHierarchy[index]));
                }
            }
            return queryHierarchyItem;
        }

        private async Task<WorkItemTrackingHttpClient> ConnectToTFS(Uri collectionUri) // Supressed the no await warning
        {
            VssConnection connection = new VssConnection(collectionUri, new VssClientCredentials());

            using (TfsTeamProjectCollection collection = new TfsTeamProjectCollection(collectionUri, new VssClientCredentials()))
            {
                collection.EnsureAuthenticated();
                this.hyperLinkService =
                   collection.GetService<TswaClientHyperlinkService>();
            }

            return connection.GetClient<WorkItemTrackingHttpClient>();
        }

        // TODO: Make this query configurable
        private async Task<List<StatusRecord>> MapQueryResultToStatusRecords(WorkItemQueryResult queryResult) 
        {
            var StatusRecordlist = new List<StatusRecord>();

            var count = 1;
            foreach (WorkItemLink item in queryResult.WorkItemRelations)
            {
                if (item.Source != null)
                {
                    var workItemInstance = this.witClient.GetWorkItemAsync(item.Target.Id).Result;
                    var parentItemInstance = this.witClient.GetWorkItemAsync(item.Source.Id).Result;

                    Enum.TryParse(workItemInstance.Fields["System.State"].ToString().Replace(" ", string.Empty), out CurrentStatus workItemStatus);

                    var record = new StatusRecord()
                    {
                        SerialNumber = count++,
                        ParentIdWithLink = new IdWithLink()
                        {
                            Id = item.Source.Id,
                            Link = this.hyperLinkService.GetWorkItemEditorUrl(item.Source.Id).ToString()
                        },
                        TaskIdWithLink = new IdWithLink()
                        {
                            Id = item.Target.Id,
                            Link = this.hyperLinkService.GetWorkItemEditorUrl(item.Target.Id).ToString()
                        },
                        TaskTitle = workItemInstance.Fields["System.Title"].ToString(),
                        TaskStatus = workItemStatus,
                        AssignedTo = ((IdentityRef)workItemInstance.Fields["System.AssignedTo"]).DisplayName,
                        ParentTitle = parentItemInstance?.Fields["System.Title"]?.ToString(),
                        CompletedWork = workItemInstance.Fields.Keys.Contains("Microsoft.VSTS.Scheduling.CompletedWork") ? workItemInstance?.Fields["Microsoft.VSTS.Scheduling.CompletedWork"]?.ToString() : string.Empty
                    };

                    StatusRecordlist.Add(record);
                }
            }
            return StatusRecordlist;

        }

        private void PrintStatusRecords(List<StatusRecord> statusListToPrint)
        {
            foreach (StatusRecord item in statusListToPrint)
            {
                Console.WriteLine($"{item.SerialNumber}  | " +
                    $"{item.ParentTitle}  | " +
                    $"{item.TaskIdWithLink.Id} | " +
                    $"{item.TaskTitle} | " +
                    $"{item.TaskStatus} | " +
                    $"{item.AssignedTo}");
            }
        }
    }
}
