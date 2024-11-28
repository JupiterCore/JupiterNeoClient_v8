using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;


using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;



namespace JpCommon
{
    public class BaseHttp
    {
        public string baseURL { get; set; } = String.Empty;
        protected static readonly HttpClient client = new HttpClient();

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

        public Task<HttpResponseMessage> get(string path, Dictionary<string, string> queryParams)
        {
            return client.GetAsync(this.BuildQueryParams(path, queryParams));
        }

        public Task<HttpResponseMessage> get(string path)
        {
            string fullPath = baseURL + path;
            return client.GetAsync(baseURL + path);
        }

        public Task<HttpResponseMessage> post(string path, object data)
        {
            var jsonData = toJSON(data);
            return client.PostAsync(baseURL + path, jsonData);
        }

        public Task put(string path, object data)
        {
            var jsonData = toJSON(data);
            return client.PutAsync(baseURL + path, jsonData);
        }

        public Task delete(string path)
        {
            return client.PutAsync(baseURL + path, null);
        }

        public async Task<dynamic> getJSON(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseBody);
            return result ?? throw new JsonException("Deserialization returned null");
        }
    }
}
