using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ServiceNowTeamFoundationSync.BDO;
using System.Configuration;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServiceNowTeamFoundationSync.BL.ServiceNow
{
    class ServiceNowClient
    {
 
        AppSettingsReader configReader = new AppSettingsReader();
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<SNDevelopmentItem> GetEnhancementList()
        {
            List<SNDevelopmentItem> devList = QueryServiceNowEntitiesAsync<Enhancement>().Result;
            return devList;
        }

        public SNDevelopmentItem GetEnhancement(string systemId)
        {
            SNDevelopmentItem devItem = GetServiceNowEntityAsync<Enhancement>(systemId).Result;
            return devItem;
        }

        public bool UpdateEnhancement(SNDevelopmentItem devItem)
        {
            return UpdateServiceNowEntityAsync<Enhancement>(devItem).Result;
        }

        public List<SNDevelopmentItem> GetDefectList()
        {
            List<SNDevelopmentItem> devList = QueryServiceNowEntitiesAsync<Defect>().Result;
            return devList;
        }
        public SNDevelopmentItem GetDefect(string systemId)
        {
            SNDevelopmentItem devItem = GetServiceNowEntityAsync<Defect>(systemId).Result;
            return devItem;
        }

        public bool UpdateDefect(SNDevelopmentItem devItem)
        {
            return UpdateServiceNowEntityAsync<Defect>(devItem).Result;
        }

        private async Task<List<SNDevelopmentItem>> QueryServiceNowEntitiesAsync<T>() where T: SNCommon
        {
            T queryResult = default(T);
            List<SNDevelopmentItem> devList = new List<SNDevelopmentItem>();
            string snTableName = "";
            if (typeof(T) == typeof(Enhancement))
            {
                snTableName = "rm_enhancement";
            }
            if (typeof(T) == typeof(Defect))
            {
                snTableName = "rm_defect";
            }
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string targetBaseUrl = (string)configReader.GetValue("ServiceNowBaseUrl", typeof(string));
                    string authUserName = (string)configReader.GetValue("ServiceNowUser", typeof(string));
                    string authPassword = (string)configReader.GetValue("ServiceNowPassword", typeof(string));
                    logger.Debug(string.Format("Retrieving Enhancement List from {0}", targetBaseUrl));
                    //Initialize Uri for HttpClient
                    httpClient.BaseAddress = new Uri(targetBaseUrl);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    //Http authententification
                    var authToken = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", authUserName, authPassword));

                    //Build the request message
                    HttpRequestMessage requestMessage = new HttpRequestMessage();
                    requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    requestMessage.RequestUri = new Uri(string.Format(@"{0}/table/{1}?sysparm_display_value=true&sysparm_view=NAV", targetBaseUrl, snTableName));
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                    requestMessage.Method = HttpMethod.Get;

                    //Query ServiceNow REST API
                    HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

                    //
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        string responseText = await responseMessage.Content.ReadAsStringAsync();
                        responseText = responseText.Replace("u_fet_company2.u_product_line", "product_line").Replace("u_fet_company2.u_segment", "segment");
                        //queryResult = JsonConvert.DeserializeObject<T>(responseText);
                        // Deserialize Json response into an enhancement
                        JObject snQueryResult = JObject.Parse(responseText);
                        IList<JToken> results = snQueryResult["result"].Children().ToList();
                        IList<T> snRecords = new List<T>();
                        foreach(var item in results)
                        {
                            T temp = item.ToObject<T>();
                            if (temp != null)
                            {
                                devList.Add(new SNDevelopmentItem()
                                {
                                    SystemId = temp.Sys_Id,
                                    Number = temp.Number,
                                    Assignment_Group = temp.Assignment_Group != null ? temp.Assignment_Group.Display_Value : "",
                                    State = temp.State,
                                    Short_Description = temp.Short_Description,
                                    External_Reference_Id = temp.Correlation_Id
                                });
                            }
                        }
                        results.Clear();
                        results = null;
                    }
                    else
                    {
                        logger.Error(string.Format(@"The response from ServiceNow is: {0}", responseMessage.ReasonPhrase));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return devList;
        }

        private async Task<SNDevelopmentItem> GetServiceNowEntityAsync<T>(string systemId) where T : SNCommon
        {
            SNDevelopmentItem snEntity = null;
            string snTableName = "";
            if (typeof(T) == typeof(Enhancement))
            {
                snTableName = "rm_enhancement";
            }
            if (typeof(T) == typeof(Defect))
            {
                snTableName = "rm_defect";
            }
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string targetBaseUrl = (string)configReader.GetValue("ServiceNowBaseUrl", typeof(string));
                    string authUserName = (string)configReader.GetValue("ServiceNowUser", typeof(string));
                    string authPassword = (string)configReader.GetValue("ServiceNowPassword", typeof(string));

                    httpClient.BaseAddress = new Uri(targetBaseUrl);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    //Http authententification
                    var authToken = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", authUserName, authPassword));

                    //Build the request message
                    HttpRequestMessage requestMessage = new HttpRequestMessage();
                    requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    requestMessage.RequestUri = new Uri(string.Format(@"{0}/table/{1}/{2}?sysparm_display_value=true&sysparm_view=NAV", targetBaseUrl, snTableName, systemId));
                     requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                    requestMessage.Method = HttpMethod.Get;
                    HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        string responseText = await responseMessage.Content.ReadAsStringAsync();
                        responseText = responseText.Replace("u_fet_company2.u_product_line", "product_line").Replace("u_fet_company2.u_segment", "segment");

                        // Deserialize Json response into an enhancement
                        JObject snQueryResult = JObject.Parse(responseText);
                        var enhancement = snQueryResult["result"].ToObject<T>();
                        if (enhancement != null)
                        {
                            snEntity = new SNDevelopmentItem()
                            {
                                SystemId = enhancement.Sys_Id,
                                Number = enhancement.Number,
                                Assigned_To = enhancement.Assignment_Group != null ? enhancement.Assignment_Group.Display_Value : "",
                                State = enhancement.State,
                                Short_Description = enhancement.Short_Description
                            };
                        }
                    }
                    else
                    {
                        logger.Error(string.Format(@"The response from ServiceNow is: {0}", responseMessage.ReasonPhrase));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return snEntity;
        }

        private async Task<bool> UpdateServiceNowEntityAsync<T>(SNDevelopmentItem devItem) where T : SNCommon
        {
            bool retValue = false;
            string snTableName = "";
            SNCommon snRecord = new SNCommon()
            {
                Short_Description = devItem.Short_Description,
                State = devItem.State,
                Correlation_Id = devItem.External_Reference_Id,
                Assignment_Group = string.IsNullOrEmpty(devItem.Assignment_Group) ?  null : new SNLookup() { Display_Value = devItem.Assignment_Group }
            };
            if (typeof(T) == typeof(Enhancement))
            {
                snTableName = "rm_enhancement";
            }
            if (typeof(T) == typeof(Defect))
            {
                snTableName = "rm_defect";
            }
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string targetBaseUrl = (string)configReader.GetValue("ServiceNowBaseUrl", typeof(string));
                    string authUserName = (string)configReader.GetValue("ServiceNowUser", typeof(string));
                    string authPassword = (string)configReader.GetValue("ServiceNowPassword", typeof(string));

                    httpClient.BaseAddress = new Uri(targetBaseUrl);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    //Http authententification
                    var authToken = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", authUserName, authPassword));

                    //Build the request message
                    string jsonContent = JsonConvert.SerializeObject(snRecord, Formatting.None,
                        new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore,
                                                       ContractResolver = new LowerCasePropertyNamesContractResolver(),
                                                       Converters = { new FormatSNLookupAsTextConverter()}
                        });
                    HttpRequestMessage requestMessage = new HttpRequestMessage();
                    requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    requestMessage.RequestUri = new Uri(string.Format(@"{0}/table/{1}/{2}?sysparm_display_value=true&sysparm_input_display_value=true&sysparm_view=NAV", targetBaseUrl, snTableName,devItem.SystemId));
                    requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    var testContent = requestMessage.ToString();
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                    requestMessage.Method = HttpMethod.Put;
                    HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        retValue = true;
                    }
                    else
                    {
                        logger.Error(string.Format(@"Could not update {0}: {1}", devItem.Number,responseMessage.ReasonPhrase));
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
