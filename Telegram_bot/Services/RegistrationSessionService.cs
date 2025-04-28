using System.Collections.Concurrent;

namespace TelegramMenuBot.Services
{
    public static class RegistrationSessionService
    {
        private static readonly ConcurrentDictionary<long, string> _fullNames = new();

        public static Task SetFullNameAsync(long chatId, string fullName)
        {
            _fullNames[chatId] = fullName;
            return Task.CompletedTask;
        }

        public static Task<string?> GetFullNameAsync(long chatId)
        {
            _fullNames.TryGetValue(chatId, out var fullName);
            return Task.FromResult(fullName);
        }

        public static Task ClearSessionAsync(long chatId)
        {
            _fullNames.TryRemove(chatId, out _);
            return Task.CompletedTask;
        }
    }
}
