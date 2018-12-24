using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Units;

using Newtonsoft.Json;

namespace Enivate.ResponseHub.Responsys.UI.Services
{
    public class ResponseHubApiService
    {

        private readonly string _serviceEndpointUrl;
        private readonly string _serviceApiKey;

        public ResponseHubApiService()
        {
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["ResponseHubService.Endpoint"]))
            {
                throw new ApplicationException("'ResponseHubService.Endpoint' configuration setting is not set.");
            }
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["ResponseHubService.ApiKey"]))
            {
                throw new ApplicationException("'ResponseHubService.ApiKey' configuration setting is not set.");
            }

            // set the settings
            _serviceApiKey = ConfigurationManager.AppSettings["ResponseHubService.ApiKey"];
            _serviceEndpointUrl = ConfigurationManager.AppSettings["ResponseHubService.Endpoint"];
        }


        public async Task<Unit> GetUnit(Guid unitId)
        {

            // Define the url
            string url = String.Format("{0}/units/{1}", _serviceEndpointUrl, unitId);

            HttpClient client = new HttpClient();
            HttpRequestMessage request = GetRequestMessage(url);
            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Read the response
            string responseString = await response.Content.ReadAsStringAsync();

            // Convert the response string to a unit
            Unit unit = JsonConvert.DeserializeObject<Unit>(responseString);

            return unit;


        }

        public async Task<IList<JobMessage>> GetLatestMessages(Guid unitId)
        {

            // Define the url
            string url = String.Format("{0}/job-messages/unit/{1}", _serviceEndpointUrl, unitId);

            HttpClient client = new HttpClient();
            HttpRequestMessage request = GetRequestMessage(url);
            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Read the response
            string responseString = await response.Content.ReadAsStringAsync();

            // Convert the response string to a unit
            IList<JobMessage> jobMessages = JsonConvert.DeserializeObject<IList<JobMessage>>(responseString);

            return jobMessages;
        }

        /// <summary>
        /// Gets the request message for the service request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private HttpRequestMessage GetRequestMessage(string url)
        {
            HttpRequestMessage message = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("APIKEY", _serviceApiKey);
            return message;
        }

    }
}
