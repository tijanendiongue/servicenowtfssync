using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceNowTeamFoundationSync.BDO
{
    public class TFSWorkItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        //public string ServiceNowNumber { get; set; }
        public string State { get; set; }
    }

    public class TFSBug : TFSWorkItem
    {
        public string ServiceNowDefect { get; set; }
        public string ServiceNowInternalId { get; set; }
    }

    public class TFSProductBacklogItem:TFSWorkItem
    {
        public string ServiceNowEnhancement { get; set; }
        public string ServiceNowInternalId { get; set; }
    }
   
}
