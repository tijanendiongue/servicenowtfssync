using System;

namespace ServiceNowTeamFoundationSync.BDO
{
    public class SNDevelopmentItem
    {
        public string Number { get; set; }
        public SNPriority Priority { get; set; }
        public string State { get; set; }
        public string Short_Description { get; set; }
        public string Description { get; set; }
        public string Segment { get; set; }
        public string Assignment_Group { get; set; }
        public string Assigned_To { get; set; }
        public string System { get; set; }
        public string Product_Line { get; set; }
        public string Navision_Company { get; set; }
        public string Module { get; set; }
        public string Change_Request { get; set; }
        public string External_Reference_Id { get; set; }
        public string SystemId { get; set; }





    }

    public enum SNPriority
    {
        Critical = 1,
        High = 2,
        Medium = 3,
        Low = 4,
        Very_Low = 5
    }

    //public enum SNEnhancementState
    //{
    //    Draft,
    //    Scoping,
    //    Awaiting_Approval,
    //    Work_In_Progress,
    //    Testing_QA,
    //    Deploy,
    //    Closed,
    //    On_Hold,
    //    Cancelled
    //}
}
