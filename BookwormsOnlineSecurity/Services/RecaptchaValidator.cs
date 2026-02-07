using System.Net.Http.Json;
using BookwormsOnlineSecurity.Models;
using Microsoft.Extensions.Options;

namespace BookwormsOnlineSecurity.Services
{
    public class RecaptchaValidator : IRecaptchaValidator
    {
        private readonly HttpClient _httpClient;
        private readonly RecaptchaSettings _settings;

        public RecaptchaValidator(HttpClient httpClient, IOptions<RecaptchaSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<bool> IsValidAsync(string token, string action, string? remoteIp = null)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(_settings.SecretKey))
                return false;

            var response = await _httpClient.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = _settings.SecretKey,
                    ["response"] = token,
                    ["remoteip"] = remoteIp ?? string.Empty
                }));

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();
            if (result == null) return false;

            // Require success and matching action; score threshold 0.5
            return result.success && string.Equals(result.action, action, StringComparison.OrdinalIgnoreCase) && result.score >= 0.5m;
        }

        private class RecaptchaResponse
        {
            public bool success { get; set; }
            public decimal score { get; set; }
            public string action { get; set; } = string.Empty;
            public DateTime challenge_ts { get; set; }
            public string hostname { get; set; } = string.Empty;
            public List<string>? error_codes { get; set; }
        }
    }
}
