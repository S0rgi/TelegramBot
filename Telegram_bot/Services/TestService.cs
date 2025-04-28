using System.Net.Http.Json;
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
        public static async Task<List<QuestionModel>> GetQuestionsByTopicAsync(string topicTitle)
        {
            var response = await _httpClient.GetAsync($"TestTitle/{Uri.EscapeDataString(topicTitle)}");

            response.EnsureSuccessStatusCode();

            var questions = await response.Content.ReadFromJsonAsync<List<QuestionModel>>();

            return questions ?? [];
        }
    }
}
