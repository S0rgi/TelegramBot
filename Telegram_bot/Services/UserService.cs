using System.Net.Http.Json;
using TelegramMenuBot.Models; // Если нужно для модели профиля

namespace TelegramMenuBot.Services
{
    public static class UserService
    {
        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("http://localhost:5135/api/v1/")
        };

        public static async Task RegisterUserAsync(long chatId, string fullName, string phoneNumber)
        {
            var payload = new
            {
                chatId,
                fullName,
                phoneNumber
            };

            var response = await _httpClient.PostAsJsonAsync("User", payload);

            if ( !response.IsSuccessStatusCode )
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при регистрации пользователя: {error}");
            }
        }

        public static async Task<bool> CheckIfUserExistsByChatIdAsync(long chatId)
        {
            var response = await _httpClient.GetAsync($"User/{chatId}/userInfo");

            if ( response.StatusCode == System.Net.HttpStatusCode.NotFound )
                return false;

            if ( response.IsSuccessStatusCode )
                return true;

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ошибка при проверке пользователя: {error}");
        }




        public static async Task<UserProfileResponse> GetUserProfileByChatIdAsync(long chatId)
        {
            var response = await _httpClient.GetAsync($"User/{chatId}/userInfo");

            if ( response.StatusCode == System.Net.HttpStatusCode.NotFound )
            {
                throw new Exception("Профиль пользователя не найден.");
            }

            if ( !response.IsSuccessStatusCode )
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка при получении профиля пользователя: {error}");
            }

            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            if ( profile == null )
            {
                throw new Exception("Не удалось прочитать данные профиля.");
            }

            return profile;
        }

    }
}
