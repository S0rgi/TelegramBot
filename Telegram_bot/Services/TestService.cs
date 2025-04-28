using System.Net.Http.Json;
using System.Text.Json;
using TelegramMenuBot.Models;

namespace TelegramMenuBot.Services
{
    public static class TestService
    {
        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("http://localhost:5135/api/v1/")
        };

        public static async Task<List<string>> GetTestTitlesAsync()
        {
            var response = await _httpClient.GetAsync("TestTitle");

            response.EnsureSuccessStatusCode();

            var titles = await response.Content.ReadFromJsonAsync<List<TestTitleSimpleResponse>>();

            return titles?.Select(t => t.Title).Where(title => !string.IsNullOrWhiteSpace(title)).Distinct().ToList() ?? new List<string>();
        }
        public static async Task<string> GetTopicDetailsAsync(string title)
        {
            var encodedTitle = Uri.EscapeDataString(title);
            var response = await _httpClient.GetAsync($"TestTitle/{encodedTitle}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        public static async Task<List<QuestionModel>> GetTopicQuestionsAsync(string title)
        {
            var encodedTitle = Uri.EscapeDataString(title);
            var response = await _httpClient.GetAsync($"TestTitle/{encodedTitle}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var questions = JsonSerializer.Deserialize<List<QuestionModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return questions ?? new List<QuestionModel>();
        }
    }
}
