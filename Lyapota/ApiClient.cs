using System.Text;
using System.Text.Json;

namespace Lyapota
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult> SendAsync(HttpMethod method, string path, object? body = null)
        {
            try
            {
                using var request = new HttpRequestMessage(method, path);
                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                using var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                return new ApiResult
                {
                    StatusCode = (int)response.StatusCode,
                    StatusText = response.StatusCode.ToString() ?? string.Empty,
                    Body = responseBody
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResult 
                { 
                    Error = $"Network error: {ex.Message}" 
                };
            }
            catch (TaskCanceledException)
            {
                return new ApiResult 
                { 
                    Error = "Timeout error." 
                };
            }
        }

        public async Task<ApiResult> GetUsersAsync()
        {
            var path = "/users";
            return await SendAsync(HttpMethod.Get, path);
        }

        public async Task<ApiResult> GetUserAsync(int userId)
        {
            var path = $"/users/{userId}";
            return await SendAsync(HttpMethod.Get, path);
        }

        public async Task<ApiResult> CreateUserAsync( object user)
        {
            var path = "/users";
            return await SendAsync(HttpMethod.Post, path, user);
        }

        public async Task<ApiResult> DeleteUserAsync(int userId)
        {
            var path = $"/users/{userId}";
            return await SendAsync(HttpMethod.Delete, path);
        }

        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
