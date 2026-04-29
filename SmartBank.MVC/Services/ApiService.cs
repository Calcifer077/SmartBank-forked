using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartBank.MVC.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ApiService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, ILogger<ApiService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
        {
            SetToken(token);
            var response = await _http.GetAsync(endpoint);
            return await ParseResponse<T>(response);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object body, string? token = null)
        {
            SetToken(token);
            var content = Serialize(body);
            var response = await _http.PostAsync(endpoint, content);
            return await ParseResponse<T>(response);
        }

        public async Task<T?> PutAsync<T>(string endpoint, object body, string? token = null)
        {
            SetToken(token);
            var content = Serialize(body);
            var response = await _http.PutAsync(endpoint, content);
            return await ParseResponse<T>(response);
        }

        // ── Helpers ───────────────────────────────────────────────────
        private void SetToken(string? token)
        {
            _http.DefaultRequestHeaders.Authorization = token is not null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
        }

        private static StringContent Serialize(object body) =>
            new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        private async Task<T?> ParseResponse<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API call failed: {Status} — {Body}",
                    response.StatusCode, json);
                return default;
            }
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
    }
}