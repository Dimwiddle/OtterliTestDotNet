using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text;
using System.Runtime.InteropServices;

namespace OtterliAPI
{
    class API
    {
        public string host {get; set;}

        
        public HttpClient client = new HttpClient();
        public object response { get; set; }

        private string version { get; set;}

        private string token { get; set; }

        public API(string token = null, string version = "1.0")
        {
            DotNetEnv.Env.TraversePath().Load();
            this.host = Environment.GetEnvironmentVariable("HOST");
            this.token = token;
            this.version = version;
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
            };
            this.client = new HttpClient(handler);

        }

        /// <summary>
        /// Sends a GET request to the API.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="endpoint"></param>
        /// <param name="queryParams"></param>
        /// <returns>HttpResponse</returns>
        public async Task<HttpResponseMessage> sendGETRequest(string endpoint, Dictionary<string, string> queryParams = null)
        {
            var queryString = "";
            if (queryParams != null)
            {
                
                queryString = "?";
                foreach (var (key, value) in queryParams){
                    queryString += $"{key}={value}&";
                }
                queryString = queryString.TrimEnd('&');
            }
                
            var requestUri = new Uri($"{host}/{endpoint}{queryString}");
            var request = new HttpRequestMessage(new HttpMethod("GET"), requestUri);
            request.Headers.Add("Accept", $"application/json;version={this.version}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", this.token);
            var response = await client.SendAsync(request);
            // if (response.StatusCode == HttpStatusCode.Unauthorized)
            // {
            //     var retryRequestUri = response.RequestMessage.RequestUri;
            //     var retryrequest = new HttpRequestMessage(new HttpMethod("GET"), retryRequestUri);
            //     retryrequest.Headers.Add("Accept", $"application/json;version={this.version}");
            //     retryrequest.Headers.Authorization = new AuthenticationHeaderValue("Token", this.token);
            //     response = await client.SendAsync(retryrequest);
            // }
            this.response = response;
            return response;
        }

        public async Task<HttpResponseMessage> APIHealthCheck()
        {
            var response = await client.GetAsync($"{host}/health");
            return response;
        }

        public async Task<ResponseContent> GetResponseContent()
        {
            if (this.response is HttpResponseMessage httpResponseMessage)
            {   
                var body = await httpResponseMessage.Content.ReadAsStringAsync();
                return new ResponseContent(body);
            }
            return null;
        }

        public  IDictionary<string, IEnumerable<string>> GetRequestHeaders(){
            if (this.response is HttpResponseMessage httpResponseMessage)
            {
                var headers = httpResponseMessage.Headers.ToDictionary(x => x.Key, x => x.Value);
                return headers;
            }
            return null;
        }

        public async Task<(HttpResponseMessage, JsonObject)> sendPOSTRequest(string endpoint, JsonObject data){
            var requestUri = new Uri($"{host}/{endpoint}");
            var request = new HttpRequestMessage(new HttpMethod("POST"), requestUri);
            request.Headers.Add("Accept", $"application/json;version={this.version}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", this.token);
            string json = data.ToString();
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            // if (response.StatusCode == HttpStatusCode.Unauthorized)
            // {
            //     var retryRequestUri = response.RequestMessage.RequestUri;
            //     var retryrequest = new HttpRequestMessage(new HttpMethod("POST"), retryRequestUri);
            //     retryrequest.Headers.Add("Accept", $"application/json;version={this.version}");
            //     retryrequest.Headers.Authorization = new AuthenticationHeaderValue("Token", this.token);
            //     retryrequest.Content = content;
            //     response = await client.SendAsync(retryrequest);
            // }
            this.response = response;
            var contentOut = await response.Content.ReadFromJsonAsync<JsonObject>();
            return (response, contentOut);
        }

        public async Task<JsonObject> getSummaryStats(){
            HttpResponseMessage response = await sendGETRequest("object_stats");
            Assert.That(response.IsSuccessStatusCode, $"Failed to get summary stats. Status: {response.StatusCode}");
            var content = await response.Content.ReadFromJsonAsync<JsonObject>();
            return content;
        }
    }

    /// <summary>
    /// A class to represent the response content of an Otterli API data request.
    /// </summary>
    /// <param name="responseBody"></param>
    public record ResponseContent(string responseBody)
    {
        public dynamic jsonBody = JsonConvert.DeserializeObject<dynamic>(responseBody);
        public int? count => jsonBody.count;
        public string? next => jsonBody.next;
        public string? previous => jsonBody.previous;
        public List<dynamic>? results => jsonBody.results.ToObject<List<dynamic>>();
    };
    
}
