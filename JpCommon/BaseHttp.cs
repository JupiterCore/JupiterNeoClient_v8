using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Net.Http;

namespace JpCommon
{
    public class BaseHttp
    {
        public string baseURL { get; set; } = string.Empty;
        protected static readonly HttpClient client = new HttpClient();

        public StringContent ToJSON(object data)
        {
            string jsonString = JsonConvert.SerializeObject(data);
            return new StringContent(jsonString, Encoding.UTF8, "application/json");
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

        // Método Get con parámetros de consulta (asíncrono)
        public async Task<HttpResponseMessage> Get(string path, Dictionary<string, string> queryParams)
        {
            return await client.GetAsync(this.BuildQueryParams(path, queryParams));
        }

        // Método Get sin parámetros de consulta (asíncrono)
        public async Task<HttpResponseMessage> Get(string path)
        {
            string fullPath = this.baseURL + path;
            return await client.GetAsync(fullPath);
        }

        // Método Post (asíncrono)
        public async Task<HttpResponseMessage> Post(string path, object data)
        {
            var jsonData = ToJSON(data);
            return await client.PostAsync(this.baseURL + path, jsonData);
        }

        // Método Put (asíncrono)
        public async Task<HttpResponseMessage> Put(string path, object data)
        {
            var jsonData = ToJSON(data);
            return await client.PutAsync(this.baseURL + path, jsonData);
        }

        // Método Delete (asíncrono)
        public async Task<HttpResponseMessage> Delete(string path)
        {
            return await client.DeleteAsync(this.baseURL + path);
        }

        // Método para obtener respuesta JSON de un HttpResponseMessage (asíncrono)
        public async Task<dynamic> GetJSON(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseBody);
            return result ?? throw new JsonException("Deserialization returned null");
        }
    } 
}
