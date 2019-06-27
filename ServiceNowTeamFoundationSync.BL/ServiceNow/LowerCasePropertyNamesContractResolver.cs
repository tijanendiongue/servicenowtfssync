using Newtonsoft.Json.Serialization;
namespace ServiceNowTeamFoundationSync.BL.ServiceNow
{
    public class LowerCasePropertyNamesContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLowerInvariant();
        }
    }
}
