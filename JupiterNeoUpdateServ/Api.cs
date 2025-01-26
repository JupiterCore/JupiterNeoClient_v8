using JpCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace JupiterNeoUpdateService
{
    public class Api
    {
        protected static readonly HttpClient client = new HttpClient();
        protected string baseURL { get; set; }

        public Api()
        {
            this.baseURL = JpConstants.UpdaterBaseUrl;
        }

        public StringContent toJSON(object data)
        {
            string jsonString = JsonConvert.SerializeObject(data);
            return new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
        }

        public string BuildQueryParams(string path, Dictionary<string, string> queryParams)
        {
            var builder = new UriBuilder(this.baseURL);
            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach (var kvp in queryParams)
            {
                query[kvp.Key] = kvp.Value;
            }
            builder.Query = query.ToString();
            return builder.ToString();
        }

        public Task<HttpResponseMessage> get(string path)
        {
            string fullPath = baseURL + path;
            return client.GetAsync(baseURL + path);
        }

        public async Task<dynamic> getJSON(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<dynamic>(responseBody);
        }

    }
}