
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Origam.Service.Core;
using System.Collections;
using System.Data;
using System.Xml;

namespace Origam.SigniService
{
    public class SigniServiceAgent : IExternalServiceAgent
    {
        private object _result;
        public object Result => _result;
        public virtual Hashtable Parameters { get; } = new Hashtable();
        public string MethodName { get; set; }
        public string TransactionId { get; set; } = null;

        private string apikey = "";
        public void Run()
        {
            switch (MethodName)
            {
                case "SendToSigni":
                    CheckParameters();
                    _result =
                        SendToSigni();
                    break;
            }
        }

        private void CheckParameters()
        {
            apikey = Environment.GetEnvironmentVariable(variable: "apikey")??"";
            if (apikey == "")
            {
                throw new Exception("Api-key does not set");
            }
            if (Parameters["signing_order"] == null)
            {
                throw new Exception("signing_order can not be empty.");
            }
            if (Parameters["autosign_proposers"] == null)
            {
                throw new Exception("autosign_proposers can not be empty.");
            }
            if (Parameters["people"] == null)
            {
                throw new Exception("people can not be empty.");
            }
            if (Parameters["template"] == null)
            {
                throw new Exception("template can not be empty.");
            }
        }

        private XmlDocument SendToSigni()
        {
            try
            {
                string jsonResult = SendPostAsync().Result;
                return JsonConvert.DeserializeXmlNode(jsonResult, "ROOT") ?? new XmlDocument();
            }catch (Exception ex)
            {
                string xmlMessage = String.Format(
              "<ROOT><code>500</code><errorCode></errorCode>" +
              "<message>{0}</message></ROOT><error/>",ex.Message);
                XmlDocument doc = new();
                doc.LoadXml(xmlMessage);
                return doc;  
            }
        }

        private async Task<string> SendPostAsync()
        {
            string requestdata = CreateRequestData();
            var baseAddress = new Uri("https://api.signi.com/");
            using var httpClient = new HttpClient { BaseAddress = baseAddress };
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", apikey);
            using var content = new StringContent(requestdata, System.Text.Encoding.Default, "application/json");
            using var response = await httpClient.PostAsync("api/v1/contract/?type=template", content);
            return await response.Content.ReadAsStringAsync();
        }

        private string CreateRequestData()
        {
            JObject json = new JObject();
            json.Add("settigns",CreateSettingsPart());
            json.Add("people",CreatePeople());
            json.Add("template",CreateTemplate());
            return json.ToString();
        }

        private JObject CreateTemplate()
        {
            var dataDocument = Parameters.Get<IDataDocument>("template");
            var templateTable = dataDocument.DataSet.Tables[0];
            var parametersTable = dataDocument.DataSet.Tables[1];

            foreach (DataRow dataRow in templateTable.Rows)
            {
                JObject templateObject = new JObject();
                var properyId = new JProperty("id", dataRow.Field<int>("TemplateId"));
                var properyName = new JProperty("name", dataRow.Field<string>("Name"));
                templateObject.Add(properyId);
                templateObject.Add(properyName);
                var arrayParam = new JArray();
                foreach (DataRow dataRowParam in parametersTable.Rows)
                {
                    JObject arrayObj = new JObject();
                    var paramId = new JProperty("id", dataRowParam.Field<int>("ParameterId"));
                    var paramValue = new JProperty("value", dataRowParam.Field<object>("ParameterValue"));
                    arrayObj.Add(paramId);
                    arrayObj.Add(paramValue);
                    arrayParam.Add(arrayObj);
                }
                templateObject.Add("parameters", arrayParam);
                return  templateObject;
            }
            return new JObject();
        }

        private JArray CreatePeople()
        {
            var dataDocument = Parameters.Get<IDataDocument>("people");
            JObject json = new JObject();
            foreach (DataTable dataTable in dataDocument.DataSet.Tables)
            {
                var arrayPeople = new JArray();
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    JObject arrayObj = new JObject();
                    var contract_role = new JProperty("contract_role", dataRow.Field<string>("Contranct"));
                    var is_proposer = new JProperty("is_proposer", dataRow.Field<bool>("Proposer"));
                    var email = new JProperty("email", dataRow.Field<object>("Email"));
                    arrayObj.Add(contract_role);
                    arrayObj.Add(is_proposer);
                    arrayObj.Add(email);
                    if (!dataRow.Field<bool>("Proposer"))
                    {
                        var party_order = new JProperty("party_order", dataRow.Field<object>("PartyOrder"));
                        var person_type = new JProperty("person_type", dataRow.Field<object>("PersonType"));
                        var first_name = new JProperty("first_name", dataRow.Field<object>("FirstName"));
                        var last_name = new JProperty("last_name", dataRow.Field<object>("LastName"));
                        arrayObj.Add(party_order);
                        arrayObj.Add(person_type);
                        arrayObj.Add(first_name);
                        arrayObj.Add(last_name);
                    }
                    arrayPeople.Add(arrayObj);
                }
                return arrayPeople;
            }
            return new JArray();
        }

        private JObject CreateSettingsPart()
        {
                JObject jsonS = new();
                    var signing_order = new JProperty("signing_order", Parameters["signing_order"]);
                    var autosign_proposers = new JProperty("autosign_proposers", Parameters["autosign_proposers"]);
                    jsonS.Add(signing_order);
                    jsonS.Add(autosign_proposers);
            return jsonS;
        }
    }
}
