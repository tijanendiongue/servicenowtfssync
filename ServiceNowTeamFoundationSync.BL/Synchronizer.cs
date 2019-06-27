using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceNowTeamFoundationSync.BDO;
using Newtonsoft.Json;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace ServiceNowTeamFoundationSync.BL
{
    using ServiceNow;
    using TeamFoundation;
    public class Synchronizer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ServiceNowClient snClient = new ServiceNowClient();
        TFSClient tfsClient = new TFSClient();
        int nbPBIUpdated;
        int nbPBICreated;
        int nbBugUpdated;
        int nbBugCreated;

        /// <summary>
        /// Gets the defects and enhancements from ServiceNow and then insert or updates them in TFS.
        /// </summary>
        public void UpdateTeamFoundation()
        {

            var EnhancementList = snClient.GetEnhancementList();
            var DefectList = snClient.GetDefectList();
            var createdPBI = InsertOrUpdatePBI(EnhancementList.ToArray());
            var createdBugs = InsertOrUpdateBug(DefectList.ToArray());
            //Update newly created TFS work item back to ServiceNow
            foreach (var item in createdPBI)
            {
                UpdateEnhancement(item);
            }
            foreach (var item in createdBugs)
            {
                UpdateDefect(item);
            }
        }

        /// <summary>
        /// Query backlog wor items in TFS and then update the corresponding items in ServiceNow. Only select WIs in state Approved or Commited.
        /// </summary>
        public void UpdateServiceNow()
        {
            //Update Enhancements with PBIs in Approved or Committed state
            var pbiList = tfsClient.GetPBIList();
            foreach(var item in pbiList)
            {
                UpdateEnhancement(item);
            }

            //Update Defects with Bugs in Approved or Committed state
            var bugList = tfsClient.GetBugList();
            foreach(var item in bugList)
            {
                UpdateDefect(item);
            }
        }

        private List<TFSProductBacklogItem> InsertOrUpdatePBI(SNDevelopmentItem[] developmentItem)
        {
            nbPBICreated = 0;
            nbPBIUpdated = 0;
            List<TFSProductBacklogItem> pbiList = new List<TFSProductBacklogItem>();
            if (developmentItem != null)
            {
                foreach (var item in developmentItem)
                {
                    var insert = InsertOrUpdateSinglePBI(item);
                    if (insert != null)
                    {
                        pbiList.Add(insert);
                    }
                }
            }
            logger.Info(string.Format("\nNumber of PBI created: {0}.\nNumber of PBI updated: {1}", nbPBICreated, nbPBIUpdated));
            return pbiList;
        }

        private List<TFSBug> InsertOrUpdateBug(SNDevelopmentItem[] developmentItem)
        {
            nbBugCreated = 0;
            nbBugUpdated = 0;
            List<TFSBug> bugList = new List<TFSBug>();
            if (developmentItem != null)
            {
                foreach (var item in developmentItem)
                {
                    var insert = InsertOrUpdateSingleBug(item);
                    if (insert != null)
                    {
                        bugList.Add(insert);
                    }
                }
            }
            logger.Info(string.Format("\nNumber of Bugs created: {0}.\nNumber of Bugs updated: {1}", nbBugCreated, nbBugUpdated));
            return bugList;
        }

        private TFSProductBacklogItem InsertOrUpdateSinglePBI(SNDevelopmentItem developmentItem)
        {
            TFSProductBacklogItem curtfsPBI = tfsClient.GetPBIFromSNDevItem(developmentItem);
            TFSProductBacklogItem tfsPBI = null;
            try
            {
                if (curtfsPBI != null)
                {
                    //Update PBI in TFS
                    try
                    {
                        tfsPBI = curtfsPBI;
                        string tfsState = Settings.Instance.SnEnhancementStateMapping.GetValueOrDefault(developmentItem.State);
                        if (!string.IsNullOrEmpty(tfsState))
                        {
                            int tfsStateOrder = Settings.Instance.TfsStateOrder.GetValueOrDefault(tfsState, 0);
                            int snStateOrder = Settings.Instance.SnEnhancementStateOrder.GetValueOrDefault(developmentItem.State, 0);
                            if (snStateOrder > tfsStateOrder)
                                tfsPBI.State = tfsState;
                        }
                        if(curtfsPBI.Title != developmentItem.Short_Description)
                        {
                            tfsPBI.Title = developmentItem.Short_Description;
                        }
                        if((tfsPBI.Title != curtfsPBI.Title) || (tfsPBI.State != curtfsPBI.State))
                        {
                            tfsClient.UpdateProductBacklogItem(developmentItem, tfsPBI);
                            nbPBIUpdated++;
                            tfsPBI = null;
                        }                                                
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
                else
                {
                    //Create PBI in TFS
                    try
                    {
                        // The project come from the mapping between Assignment Group and team project in Settings file                
                        string assginmentGroup = developmentItem.Assignment_Group == null?string.Empty: developmentItem.Assignment_Group;
                        string teamProject = Settings.Instance.SnAssignGroupMapping.GetValueOrDefault(assginmentGroup);
                        string tfsState = Settings.Instance.SnEnhancementStateMapping.GetValueOrDefault(developmentItem.State);
                        if ((tfsState == "New") && (!string.IsNullOrEmpty(teamProject)))
                        {
                            tfsPBI = tfsClient.CreateProductBacklogItem(developmentItem, teamProject);
                            nbPBICreated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return tfsPBI;
        }

        public TFSBug InsertOrUpdateSingleBug(SNDevelopmentItem developmentItem)
        {
            TFSBug tfsBug = null;
            TFSBug curtfsBug = tfsClient.GetBugFromSNDevItem(developmentItem);
            try
            {
                if (curtfsBug != null)
                {
                    //Update Bug in TFS
                    try
                    {
                        tfsBug = curtfsBug;
                        string tfsState = Settings.Instance.SnDefectStateMapping.GetValueOrDefault(developmentItem.State);
                        if (!string.IsNullOrEmpty(tfsState))
                        {
                            int tfsStateOrder = Settings.Instance.TfsStateOrder.GetValueOrDefault(tfsState, 0);
                            int snStateOrder = Settings.Instance.SnEnhancementStateOrder.GetValueOrDefault(developmentItem.State, 0);
                            if (snStateOrder > tfsStateOrder)
                                tfsBug.State = tfsState;
                        }
                        if(curtfsBug.Title != developmentItem.Short_Description)
                        {
                            tfsBug.Title = developmentItem.Short_Description;
                        }
                        if((tfsBug.Title != curtfsBug.Title) || (tfsBug.State != curtfsBug.State))
                        {
                            tfsClient.UpdateBug(developmentItem, tfsBug);
                            nbBugUpdated++;
                            tfsBug = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
                else
                {
                    //Create Bug in TFS
                    try
                    {
                        // The project come from the mapping between Assignment Group and team project in Settings file                
                        string assginmentGroup = developmentItem.Assignment_Group == null ? string.Empty : developmentItem.Assignment_Group;
                        string teamProject = Settings.Instance.SnAssignGroupMapping.GetValueOrDefault(assginmentGroup);
                        string tfsState = Settings.Instance.SnDefectStateMapping.GetValueOrDefault(developmentItem.State);
                        if ((tfsState == "New") &&(!string.IsNullOrEmpty(teamProject)))
                        {
                            tfsBug = tfsClient.CreateBug(developmentItem, teamProject);
                            nbBugCreated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return tfsBug;
        }

        private bool UpdateEnhancement(TFSProductBacklogItem backlogItem)
        {
            bool retValue = false;
            SNDevelopmentItem devItem = new SNDevelopmentItem()
            {
                Number = backlogItem.ServiceNowEnhancement,
                SystemId = backlogItem.ServiceNowInternalId,
                External_Reference_Id = string.Format("PBI{0}", backlogItem.ID),
                Short_Description = backlogItem.Title
            };
            try
            {
                var devItem2 = snClient.GetEnhancement(devItem.SystemId);
                string snState = Settings.Instance.TfsWIStateMapping.GetValueOrDefault(backlogItem.State);
                string snAssignmentGroup = devItem2.Assignment_Group;
                string assignmentGroup = Settings.Instance.TfsWIStateAssignmentGroup.GetValueOrDefault(backlogItem.State);
                if (!string.IsNullOrEmpty(snState))
                {
                    // Do not modify SN state if TFS state order is lower than ServiceNow state order
                    int tfsStateOrder = Settings.Instance.TfsStateOrder.GetValueOrDefault(backlogItem.State, 0);
                    int snStateOrder = Settings.Instance.SnEnhancementStateOrder.GetValueOrDefault(devItem2.State, 0);
                    if (snStateOrder <= tfsStateOrder)
                        devItem.State = snState;

                    // Only modify the assignment group if the state is "Work in Progress"
                    if (!string.IsNullOrEmpty(assignmentGroup) && (snState == "Work in Progress"))
                    {
                        if ((snAssignmentGroup == "Navision Development") || (snAssignmentGroup == "Navision Support"))
                        {
                            devItem.Assignment_Group = assignmentGroup;
                        }
                    }
                    if ((devItem.State != devItem2.State) || 
                        (devItem.Assignment_Group != devItem2.Assignment_Group) ||
                        (devItem.External_Reference_Id != devItem2.External_Reference_Id))
                    {
                        retValue = snClient.UpdateEnhancement(devItem);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return retValue;
        }

        private bool UpdateDefect(TFSBug backlogItem)
        {
            bool retValue = false;
            SNDevelopmentItem devItem = new SNDevelopmentItem()
            {
                Number = backlogItem.ServiceNowDefect,
                SystemId = backlogItem.ServiceNowInternalId,
                External_Reference_Id = string.Format("BUG{0}", backlogItem.ID)
            };
            try
            {
                var devItem2 = snClient.GetDefect(devItem.SystemId);
                string snState = Settings.Instance.TfsWIStateMapping.GetValueOrDefault(backlogItem.State);
                string assignmentGroup = Settings.Instance.TfsWIStateAssignmentGroup.GetValueOrDefault(backlogItem.State);
                if (!string.IsNullOrEmpty(snState))
                {
                    // Do not modify SN state if TFS state order is lower than ServiceNow state order
                    int tfsStateOrder = Settings.Instance.TfsStateOrder.GetValueOrDefault(backlogItem.State, 0);
                    int snStateOrder = Settings.Instance.SnDefectStateOrder.GetValueOrDefault(devItem2.State, 0);
                    if (snStateOrder <= tfsStateOrder)
                        devItem.State = snState;

                    // Only modify the assignment group if the state is "Work in Progress"
                    if (!string.IsNullOrEmpty(assignmentGroup) && (snState == "Work in Progress"))
                        devItem.Assignment_Group = assignmentGroup;

                    if ((devItem.State != devItem2.State) ||
                        (devItem.Assignment_Group != devItem2.Assignment_Group) ||
                        (devItem.External_Reference_Id != devItem2.External_Reference_Id))
                    {
                        retValue = snClient.UpdateDefect(devItem);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return retValue;
        }
    }
}
