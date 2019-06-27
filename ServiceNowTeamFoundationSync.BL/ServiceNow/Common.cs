using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowTeamFoundationSync.BL.ServiceNow
{
    class SNCommon
    {
        public string Sys_Id { get; set; }
        public string Number { get; set; }
        public string Priority { get; set; }
        public string State { get; set; }
        public string Short_Description { get; set; }
        public string Description { get; set; }
        public SNLookup Cmdb_CI { get; set; }
        public SNLookup U_Change_Request { get; set; }
        public SNLookup Assignment_Group { get; set; }
        public SNLookup U_FET_Company2 { get; set; }
        public string Product_Line { get; set; }
        public string Segment { get; set; }
        public string Correlation_Id { get; set; }

    }
    class SNLookup
    {
        public string Display_Value { get; set; }
        public string Link { get; set; }

    }

    sealed class FormatSNLookupAsTextConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SNLookup);
        }
        public override bool CanRead => false;

        public override bool CanWrite => true;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            SNLookup lookup = (SNLookup)value;
            writer.WriteValue(lookup.Display_Value);
        }
    }
}
