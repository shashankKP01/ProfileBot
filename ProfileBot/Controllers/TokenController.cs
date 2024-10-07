using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using ProfileBot;
using ProfileBot.Services;

namespace NewJoineeBOT.Controllers
{
    //[EnableCors(origins: "*.fnf.com", headers: "*", methods: "*")]
    [Route("api/[controller]")]
    [ApiController]

    public class TokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AzureTableHelper _azureTableHelper;
        private readonly IBotTelemetryClient _telemetryClient;
        private readonly IHttpClientFactory _httpClientFactory; //Inject IHttpClientFactory     // for faster launch of bot
        public TokenController(IConfiguration configuration, IServiceProvider services, IHttpClientFactory httpClientFactory)
        {
            //string connectionString = GetKeyVaultSecrets.ReturnConnectionString("azuretableconnectionstring");
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=profiletable;AccountKey=jIU+mepoQaoN0ZIwgZ4Yq5yN+nAwsEiHWXT1akKP+UIbSRVnC+7GtxHR5UJqHm21/n0lnfu6qtsD+AStDgNQsg==;EndpointSuffix=core.windows.net";
            _azureTableHelper = new AzureTableHelper(connectionString);
            _configuration = configuration;
            //_azureTableHelper = azureTableHelper;
            _telemetryClient = (IBotTelemetryClient)services.GetService(typeof(IBotTelemetryClient));
            _httpClientFactory = httpClientFactory; //Inject IHttpClientFactory
        }

        //GET api/<ValuesController>/5
        [Microsoft.AspNetCore.Mvc.HttpGet("fetch")]
        public async Task<DirectLineToken> GetAsync(string id)
        {
            DirectLineToken body = new DirectLineToken();
            string domain = string.IsNullOrEmpty(Request.Headers["Origin"].ToString()) ? Request.Headers["Host"].ToString() : Request.Headers["Origin"].ToString();
            this.Response.Headers.Add("Content-Type", "application/json");
            if (true)
            {
                id = System.Uri.UnescapeDataString(id);
                string token = _azureTableHelper.GetTokenIfExists(id) ?? String.Empty;

                if (string.IsNullOrEmpty(token))
                {
                    var secret = "nttvbUn3uh4.kBalexPLBOS-pKuy2RcMQIN4ZZorBsWWtNA2m5NRiZg";
                    //var secret = "Wu7RCkRzVXU.hOJbzIwz9XkK0GJjdGLLxl7L6pxZr9znajo8P6bjxc8";

                    // HttpClient client = new HttpClient();
                    var client = _httpClientFactory.CreateClient("Shashi");   // for faster launching of bot

                    HttpRequestMessage request = new HttpRequestMessage(
                      HttpMethod.Post,
                      $"https://directline.botframework.com/v3/directline/tokens/generate");
                    request.Headers.Add("Authorization", $"Bearer {secret}");
                    request.Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new { User = new { Id = "dl_" + id } }),
                        Encoding.UTF8,
                        "application/json");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        body = JsonConvert.DeserializeObject<DirectLineToken>(await response.Content.ReadAsStringAsync());
                        await saveUserIDTokenAsync(body.conversationId, body.token);
                    }
                }
                else
                    body.token = token;
                return body;
            }
            else
            {
                body.token = "Unauthorised Access - " + domain;
                return body;
            }
        }



        public class DirectLineToken
        {
            public string conversationId { get; set; }
            public string token { get; set; }
            public int expires_in { get; set; }
        }

        private async Task saveUserIDTokenAsync(string id, string token)
        {
            await _azureTableHelper.insertccbotstorageaccountAsync(id, token);
        }

    }
}