using System.Net.Http.Json;
using JpCommon;
using Microsoft.AspNetCore.WebUtilities;

namespace JupiterNeoServiceClient.Utils
{
   public class BaseHttp
    {
        private readonly HttpClient _client;
        public string BaseURL = JpConstants.ApiBaseUrl;

        public BaseHttp(HttpClient client)
        {
            _client = client;
        }

        public string BuildQueryParams(string path, Dictionary<string, string> queryParams)
        {
            var uri = new Uri(new Uri(BaseURL), path);
            return QueryHelpers.AddQueryString(uri.ToString(), queryParams!);
        }

        public async Task<HttpResponseMessage> GetAsync(string path, Dictionary<string, string>? queryParams = null)
        {
            var fullPath = queryParams != null ? BuildQueryParams(path, queryParams) : new Uri(new Uri(BaseURL), path).ToString();
            return await _client.GetAsync(fullPath);
        }

        public async Task<HttpResponseMessage> PostAsync(string path, object data)
        {
            var fullPath = new Uri(new Uri(BaseURL), path).ToString();
            return await _client.PostAsJsonAsync(fullPath, data);
        }

        public async Task<HttpResponseMessage> PutAsync(string path, object data)
        {
            var fullPath = new Uri(new Uri(BaseURL), path).ToString();
            return await _client.PutAsJsonAsync(fullPath, data);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string path)
        {
            var fullPath = new Uri(new Uri(BaseURL), path).ToString();
            return await _client.DeleteAsync(fullPath);
        }

        public async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode(); // Throws an exception if the response is not successful
            return await response.Content.ReadFromJsonAsync<T>();
        }

    }
}
