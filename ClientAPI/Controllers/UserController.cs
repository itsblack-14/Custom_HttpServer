using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    }
}
