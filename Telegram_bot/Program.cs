using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramMenuBot.Utilities;
using TelegramMenuBot.Bot;
using System.Collections.Concurrent;

namespace TelegramMenuBot
{
    internal class Program
    {
        private static ConcurrentDictionary<long, List<string>> AvailableTopics = new();
        static async Task Main(string[] args)
        {
            var botClient = BotClientProvider.Client;

            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

            botClient.StartReceiving(
                BotHandlers.HandleUpdateAsync,
                BotHandlers.HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Бот запущен.");
            Console.ReadLine();
        }
    }
}
