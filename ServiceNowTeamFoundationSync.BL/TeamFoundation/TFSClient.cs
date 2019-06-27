using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using ServiceNowTeamFoundationSync.BDO;

namespace ServiceNowTeamFoundationSync.BL.TeamFoundation
{
    class TFSClient
    {

        AppSettingsReader configReader = new AppSettingsReader();
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IList<TFSProductBacklogItem> GetPBIList()
        {
            IList<TFSProductBacklogItem> tfsPBIList = new List<TFSProductBacklogItem>();
            try
            {
                //create a wiql object and build our query
                Wiql wiql = new Wiql()
                {
                    Query = "Select [ID],[Title],[ServiceNow InternalId] " +
                            "From WorkItems " +
                            "Where [Work Item Type] = 'Product Backlog Item' " +
                            "And [ServiceNow InternalId] <> '' " +
                            "And (([State] = 'Approved') Or ([State] = 'Committed'))"
                };

                string serverUrl = (string)configReader.GetValue("TFSServerUrl", typeof(string));
                //Initialise the connection to the TFS server
                VssConnection connection = new VssConnection(new Uri(serverUrl), new VssCredentials(new WindowsCredential(true)));

                //create instance of work item tracking http client
                using (WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>())
                {
                    //execute the query to get the list of work items in teh results
                    WorkItemQueryResult workItemQueryResult = workItemTrackingHttpClient.QueryByWiqlAsync(wiql).Result;

                    //Some error handling
                    if (workItemQueryResult.WorkItems.Count() != 0)
                    {
                        //need to get the list work item Ids to put them into array
                        List<int> list = new List<int>();
                        foreach (var item in workItemQueryResult.WorkItems)
                        {
                            list.Add(item.Id);
                        }

                        int[] arr = list.ToArray();

                        //build a list of the fields we want to see
                        string[] fields = new string[5];
                        fields[0] = "System.Id";
                        fields[1] = "System.Title";
                        fields[2] = "System.State";
                        fields[3] = "FET.SNEnhancement";
                        fields[4] = "FET.SNInternalId";

                        var workItemList = workItemTrackingHttpClient.GetWorkItemsAsync(arr, fields, workItemQueryResult.AsOf).Result;

                        foreach (var workItem in workItemList)
                        {
                            tfsPBIList.Add(new TFSProductBacklogItem()
                            {
                                ID = workItem.Id.Value,
                                Title = (string)workItem.Fields[fields[1]],
                                State = (string)workItem.Fields[fields[2]],
                                ServiceNowEnhancement = (string)workItem.Fields[fields[3]],
                                ServiceNowInternalId = (string)workItem.Fields[fields[4]]
                            });
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return tfsPBIList;
        }


        public TFSProductBacklogItem GetPBIFromSNDevItem(SNDevelopmentItem developmentItem)
        {
            TFSProductBacklogItem tfsPBI = null;
            try
            {
                //create a wiql object and build our query
                Wiql wiql = new Wiql()
                {
                    Query = "Select [ID],[Title],[ServiceNow InternalId] " +
                            "From WorkItems " +
                            "Where [Work Item Type] = 'Product Backlog Item' " +
                            "And [ServiceNow InternalId] = '" + developmentItem.SystemId + "' "
                };

                string serverUrl = (string) configReader.GetValue("TFSServerUrl", typeof(string));
                //Initialise the connection to the TFS server
                VssConnection connection = new VssConnection(new Uri(serverUrl), new VssCredentials(new WindowsCredential(true)));

                //create instance of work item tracking http client
                using (WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>())
                {
                    //execute the query to get the list of work items in teh results
                    WorkItemQueryResult workItemQueryResult = workItemTrackingHttpClient.QueryByWiqlAsync(wiql).Result;

                    //Some error handling
                    if (workItemQueryResult.WorkItems.Count() != 0)
                    {
                        //need to get the work item Id for the GET request
                        int wiID = workItemQueryResult.WorkItems.First().Id;


                        //build a list of the fields we want to see
                        string[] fields = new string[5];
                        fields[0] = "System.Id";
                        fields[1] = "System.Title";
                        fields[2] = "System.State";
                        fields[3] = "FET.SNEnhancement";
                        fields[4] = "FET.SNInternalId";

                        var workItem = workItemTrackingHttpClient.GetWorkItemAsync(wiID, fields, workItemQueryResult.AsOf).Result;

                        if (workItem != null)
                        {
                            tfsPBI = new TFSProductBacklogItem()
                            {
                                ID = workItem.Id.Value,
                                Title = (string)workItem.Fields[fields[1]],
                                State = (string)workItem.Fields[fields[2]],
                                ServiceNowEnhancement = (string)workItem.Fields[fields[3]],
                                ServiceNowInternalId = (string)workItem.Fields[fields[4]]
                            };
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return tfsPBI;
        }

        public IList<TFSBug> GetBugList()
        {
            IList<TFSBug> tfsBugList = new List<TFSBug>();
            try
            {
                //create a wiql object and build our query
                Wiql wiql = new Wiql()
                {
                    Query = "Select [ID],[Title],[ServiceNow InternalId] " +
                            "From WorkItems " +
                            "Where [Work Item Type] = 'Bug' " +
                            "And [ServiceNow InternalId] <> '' " +
                            "And (([State] = 'Approved') Or ([State] = 'Committed'))"
                };

                string serverUrl = (string)configReader.GetValue("TFSServerUrl", typeof(string));
                //Initialise the connection to the TFS server
                VssConnection connection = new VssConnection(new Uri(serverUrl), new VssCredentials(new WindowsCredential(true)));

                //create instance of work item tracking http client
                using (WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>())
                {
                    //execute the query to get the list of work items in teh results
                    WorkItemQueryResult workItemQueryResult = workItemTrackingHttpClient.QueryByWiqlAsync(wiql).Result;

                    //Some error handling
                    if (workItemQueryResult.WorkItems.Count() != 0)
                    {
                        //need to get the list work item Ids to put them into array
                        List<int> list = new List<int>();
                        foreach (var item in workItemQueryResult.WorkItems)
                        {
                            list.Add(item.Id);
                        }

                        int[] arr = list.ToArray();

                        //build a list of the fields we want to see
                        string[] fields = new string[5];
                        fields[0] = "System.Id";
                        fields[1] = "System.Title";
                        fields[2] = "System.State";
                        fields[3] = "FET.SNDefect";
                        fields[4] = "FET.SNInternalId";

                        var workItemList = workItemTrackingHttpClient.GetWorkItemsAsync(arr, fields, workItemQueryResult.AsOf).Result;

                        foreach (var workItem in workItemList)
                        {
                            tfsBugList.Add(new TFSBug()
                            {
                                ID = workItem.Id.Value,
                                Title = (string)workItem.Fields[fields[1]],
                                State = (string)workItem.Fields[fields[2]],
                                ServiceNowDefect = (string)workItem.Fields[fields[3]],
                                ServiceNowInternalId = (string)workItem.Fields[fields[4]]
                            });
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return tfsBugList;
        }

        public TFSBug GetBugFromSNDevItem(SNDevelopmentItem developmentItem)
        {
            TFSBug tfsBug = null;
            try
            {
                //create a wiql object and build our query
                Wiql wiql = new Wiql()
                {
                    Query = "Select [ID],[Title],[ServiceNow InternalId] " +
                            "From WorkItems " +
                            "Where [Work Item Type] = 'Bug' " +
                            "And [ServiceNow InternalId] = '" + developmentItem.SystemId + "' "
                };

                string serverUrl = (string)configReader.GetValue("TFSServerUrl", typeof(string));
                //Initialise the connection to the TFS server
                VssConnection connection = new VssConnection(new Uri(serverUrl), new VssCredentials(new WindowsCredential(true)));

                //create instance of work item tracking http client
                using (WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>())
                {
                    //execute the query to get the list of work items in teh results
                    WorkItemQueryResult workItemQueryResult = workItemTrackingHttpClient.QueryByWiqlAsync(wiql).Result;

                    //Some error handling
                    if (workItemQueryResult.WorkItems.Count() != 0)
                    {
                        //need to get the work item Id for the GET request
                        int wiID = workItemQueryResult.WorkItems.First().Id;


                        //build a list of the fields we want to see
                        string[] fields = new string[5];
                        fields[0] = "System.Id";
                        fields[1] = "System.Title";
                        fields[2] = "System.State";
                        fields[3] = "FET.SNDefect";
                        fields[4] = "FET.SNInternalId";

                        var workItem = workItemTrackingHttpClient.GetWorkItemAsync(wiID, fields, workItemQueryResult.AsOf).Result;

                        if (workItem != null)
                        {
                            tfsBug = new TFSBug()
                            {
                                ID = workItem.Id.Value,
                                Title = (string)workItem.Fields[fields[1]],
                                State = (string)workItem.Fields[fields[2]],
                                ServiceNowDefect = (string)workItem.Fields[fields[3]],
                                ServiceNowInternalId = (string)workItem.Fields[fields[4]]
                            };
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return tfsBug;
        }

        public TFSProductBacklogItem CreateProductBacklogItem(SNDevelopmentItem developmentItem,string teamProject)
        {
            TFSProductBacklogItem tfsPBI = null;
            // Construct the object containing field values required for the new work item
            JsonPatchDocument patchDocument = new JsonPatchDocument();
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = developmentItem.Short_Description == null? "No description in ServiceNow" : developmentItem.Short_Description
                });
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/FET.SNEnhancement",
                    Value = developmentItem.Number
                }
                );
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/FET.SNInternalId",
                    Value = developmentItem.SystemId
                }
                );


            string serverUrl = (string)configReader.GetValue("TFSServerUrl", typeof(string));
            //Initialise the connection to the TFS server
            VssConnection connection = new VssConnection(new Uri(serverUrl), new VssCredentials(new WindowsCredential(true)));

            using (WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>())
            {
                // Get the project to create the work item in
                TeamProjectReference projectReference = FindProject(connection, teamProject);

                if (projectReference != null)
                {
                    // Create the new work item
                    WorkItem newWorkItem = workItemTrackingClient.CreateWorkItemAsync(patchDocument, projectReference.Id, "Product Backlog Item").Result;
                    if (newWorkItem != null)
                    {
                        tfsPBI = GetPBIFromSNDevItem(developmentItem);
                    }
                }
            }
            return tfsPBI;
        }

        public TFSProductBacklogItem UpdateProductBacklogItem(SNDevelopmentItem developmentItem, TFSProductBacklogItem tfsPBI)
        {
            // Construct the object containing field values required for the new work item
            JsonPatchDocument patchDocument = new JsonPatchDocument();
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = developmentItem.Short_Description == null ? "No description in ServiceNow" : developmentItem.Short_Description
                });
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.State",
                    Value = tfsPBI.State
                }
                );

            string serverUrl = (string)configReader.GetValue("TFSServerUrl", typeof(string));
            //Initialise the connection to the TFS server
            VssConnection connection = new VssConnection(new Uri(serverUrl), new VssCredentials(new WindowsCredential(true)));

            using (WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>())
            {

                    // Create the new work item
                    WorkItem UpdatedWorkItem = workItemTrackingClient.UpdateWorkItemAsync(patchDocument, tfsPBI.ID).Result;
                    if (UpdatedWorkItem != null)
                    {
                        tfsPBI = GetPBIFromSNDevItem(developmentItem);
                    }
            }
            return tfsPBI;
        }

        public TFSBug CreateBug(SNDevelopmentItem developmentItem, string teamProject)
        {
            TFSBug tfsBug = null;
            // Construct the object containing field values required for the new work item
            JsonPatchDocument patchDocument = new JsonPatchDocument();
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = developmentItem.Short_Description == null ? "No description in ServiceNow" : developmentItem.Short_Description
                });
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/FET.SNDefect",
                    Value = developmentItem.Number
                }
                );
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/FET.SNInternalId",
                    Value = developmentItem.SystemId
                }
                );


            string serverUrl = (string)configReader.GetValue("TFSServerUrl", typeof(string));
            //Initialise the connection to the TFS server
            VssConnection connection = new VssConnection(new Uri(serverUrl), new VssCredentials(new WindowsCredential(true)));

            using (WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>())
            {
                // Get the project to create the work item in
                TeamProjectReference projectReference = FindProject(connection, teamProject);

                if (projectReference != null)
                {
                    // Create the new work item
                    WorkItem newWorkItem = workItemTrackingClient.CreateWorkItemAsync(patchDocument, projectReference.Id, "Bug").Result;

                    if (newWorkItem != null)
                    {
                        tfsBug = GetBugFromSNDevItem(developmentItem);
                    }
                }
            }
            return tfsBug;
        }

        public TFSBug UpdateBug(SNDevelopmentItem developmentItem, TFSBug tFSBug)
        {
            // Construct the object containing field values required for the new work item
            JsonPatchDocument patchDocument = new JsonPatchDocument();
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = developmentItem.Short_Description == null ? "No description in ServiceNow" : developmentItem.Short_Description
                });
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.State",
                    Value = tFSBug.State
                }
                );

            string serverUrl = (string)configReader.GetValue("TFSServerUrl", typeof(string));
            //Initialise the connection to the TFS server
            VssConnection connection = new VssConnection(new Uri(serverUrl), new VssCredentials(new WindowsCredential(true)));

            using (WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>())
            {

                // Create the new work item
                WorkItem UpdatedWorkItem = workItemTrackingClient.UpdateWorkItemAsync(patchDocument, tFSBug.ID).Result;
                if (UpdatedWorkItem != null)
                {
                    tFSBug = GetBugFromSNDevItem(developmentItem);
                }
            }
            return tFSBug;
        }

        private TeamProjectReference FindProject(VssConnection connection, string projectName)
        {
            TeamProjectReference teamProject = null;
            ProjectHttpClient projectHttpClient = connection.GetClient<ProjectHttpClient>();
            teamProject = (from p in projectHttpClient.GetProjects(null).Result
                           where p.Name == projectName
                           select p).FirstOrDefault();

            return teamProject;
        }
    }
}

