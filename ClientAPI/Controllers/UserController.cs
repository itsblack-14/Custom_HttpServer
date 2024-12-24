using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace ClientAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            string customServerUrl = "http://localhost:8080/api/users";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(customServerUrl);

                //response.EnsureSuccessStatusCode();
                if(!response.IsSuccessStatusCode) 
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed");
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                return Ok(responseBody);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetail(string id)
        {
            string customServerUrl = $"http://localhost:8080/api/users/{id}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(customServerUrl);

                string responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, responseBody);
                }
                return Ok(responseBody);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody]User req)
        {
            string customServerUrl = $"http://localhost:8080/api/users/";

            try
            {
                string json = JsonSerializer.Serialize(req); // Serialize object to JSON
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(customServerUrl, content);

                string responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, responseBody);
                }
                return Ok(responseBody);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }

    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
