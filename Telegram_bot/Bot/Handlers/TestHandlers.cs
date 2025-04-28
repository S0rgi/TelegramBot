using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram_bot.Utilities;
using TelegramMenuBot.Bot;
using TelegramMenuBot.Services;

namespace Telegram_bot.Bot.Handlers
{
    public static class TestHandlers
    {
        public static async Task<bool> HandleMaterialsAsync(ITelegramBotClient botClient, Message message, long chatId)
        {
            if ( message.Text != "📚 Материалы" )
                return false;

            var titles = await TestService.GetTestTitlesAsync();
            if ( titles.Count == 0 )
            {
                await botClient.SendMessage(chatId, "❗ Тем пока нет.", replyMarkup: BotReplyKeyboards.GetMainMenu());
            }
            else
            {
                var keyboard = BotReplyKeyboards.GetTopicsKeyboard(titles);
                await botClient.SendMessage(chatId, "📚 Выберите тему для изучения материалов:", replyMarkup: keyboard);

                Sessions.PendingTestTopics[chatId] = "materials";
            }

            return true;
        }
        public static async Task<bool> HandleTestsAsync(ITelegramBotClient botClient, Message message, long chatId)
        {
            if ( message.Text != "📝 Тесты" )
                return false;

            var titles = await TestService.GetTestTitlesAsync();
            if ( titles.Count == 0 )
            {
                await botClient.SendMessage(chatId, "❗ Тем пока нет.", replyMarkup: BotReplyKeyboards.GetMainMenu());
            }
            else
            {
                var keyboard = BotReplyKeyboards.GetTopicsKeyboard(titles);
                await botClient.SendMessage(chatId, "📝 Выберите тему для прохождения теста:", replyMarkup: keyboard);

                Sessions.PendingTestTopics[chatId] = "tests";
            }

            return true;
        }

    }
}
