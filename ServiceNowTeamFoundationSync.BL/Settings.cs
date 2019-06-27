using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ServiceNowTeamFoundationSync.BL
{
    public sealed class Settings
    {
        private static Settings instance = null;
        private static readonly object padlock = new object();
        Dictionary<string,string> snAssignGroupMapping = new Dictionary<string, string>();
        Dictionary<string,string> snDefectStateMapping = new Dictionary<string, string>();
        Dictionary<string,string> snEnhancementStateMapping = new Dictionary<string, string>();
        Dictionary<string, string> tfsWIStateMapping = new Dictionary<string, string>();
        Dictionary<string, string> tfsWIStateAssignmentGroup = new Dictionary<string, string>();
        Dictionary<string, int> snEnhancementStateOrder = new Dictionary<string, int>();
        Dictionary<string, int> snDefectStateOrder = new Dictionary<string, int>();
        Dictionary<string, int> tfsStateOrder = new Dictionary<string, int>();

        Settings()
        {

        }

        public static Settings Instance
        {
            get
            {
                lock (padlock)
                {
                    if(instance == null)
                    {
                        //instance = new Settings();
                        JsonSerializer serializer = new JsonSerializer();
                        using (StreamReader sr = new StreamReader(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.ToString(), @"AppSettings.json")))
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            instance = serializer.Deserialize<Settings>(reader);
                        }
                    }
                    return instance;
                }
            }
        }

        public Dictionary<string, string> SnAssignGroupMapping { get => snAssignGroupMapping; set => snAssignGroupMapping = value; }
        public Dictionary<string, string> SnDefectStateMapping { get => snDefectStateMapping; set => snDefectStateMapping = value; }
        public Dictionary<string, string> SnEnhancementStateMapping { get => snEnhancementStateMapping; set => snEnhancementStateMapping = value; }
        public Dictionary<string, string> TfsWIStateMapping { get => tfsWIStateMapping; set => tfsWIStateMapping = value; }
        public Dictionary<string, string> TfsWIStateAssignmentGroup { get => tfsWIStateAssignmentGroup; set => tfsWIStateAssignmentGroup = value; }
        public Dictionary<string, int> SnEnhancementStateOrder { get => snEnhancementStateOrder; set => snEnhancementStateOrder = value; }
        public Dictionary<string, int> SnDefectStateOrder { get => snDefectStateOrder; set => snDefectStateOrder = value; }
        public Dictionary<string, int> TfsStateOrder { get => tfsStateOrder; set => tfsStateOrder = value; }
    }

    /// <summary>
    /// Gets a value of dictionnary. If getting the value failed return the default value for TValue type.
    /// </summary>
    public static class Extensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
