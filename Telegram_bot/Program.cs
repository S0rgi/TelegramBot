using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramMenuBot.Utilities;
using TelegramMenuBot.Bot;

namespace TelegramMenuBot
{
    internal class Program
    {
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
