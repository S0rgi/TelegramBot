using Telegram.Bot;

namespace TelegramMenuBot.Utilities
{
    public static class BotClientProvider
    {
        public static TelegramBotClient Client { get; } = new TelegramBotClient("BotToketn");
    }
}
