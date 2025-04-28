using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramMenuBot.Services;
using TelegramMenuBot.Bot;
using TelegramMenuBot.Utilities;

namespace TelegramMenuBot.Bot
{
    public static class BotHandlers
    {
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if ( update.Type != UpdateType.Message || update.Message == null )
                return;

            var message = update.Message;
            var chatId = message.Chat.Id;

            if ( message.Text == "/start" )
            {
                bool userExists = await UserService.CheckIfUserExistsByChatIdAsync(chatId);

                if ( userExists )
                {
                    await botClient.SendMessage(chatId, "📋 Главное меню:", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }
                else
                {
                    await botClient.SendMessage(chatId, "👋 Привет! Введите своё ФИО для регистрации:");
                }
                return;
            }

            bool isRegistered = await UserService.CheckIfUserExistsByChatIdAsync(chatId);

            if ( !isRegistered )
            {
                await HandleRegistrationAsync(botClient, message, chatId);
                return;
            }

            await HandleAuthorizedUserAsync(botClient, message, chatId);
        }

        private static async Task HandleRegistrationAsync(ITelegramBotClient botClient, Message message, long chatId)
        {
            if ( message.Contact?.PhoneNumber != null )
            {
                string phoneNumber = message.Contact.PhoneNumber;

                string? fullName = await RegistrationSessionService.GetFullNameAsync(chatId);
                if ( fullName == null )
                {
                    await botClient.SendMessage(chatId, "🚫 Пожалуйста, сначала введите ФИО. Введите /start");
                    return;
                }

                try
                {
                    await UserService.RegisterUserAsync(chatId, fullName, phoneNumber);
                    await botClient.SendMessage(chatId, "✅ Регистрация завершена!", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }
                catch ( Exception ex )
                {
                    Console.WriteLine($"Ошибка при регистрации: {ex.Message}");
                    await botClient.SendMessage(chatId, "⚠️ Ошибка при регистрации. Попробуйте позже.");
                }

                await RegistrationSessionService.ClearSessionAsync(chatId);
            }
            else
            {
                await RegistrationSessionService.SetFullNameAsync(chatId, message.Text);
                await botClient.SendMessage(chatId, "📱 Теперь отправьте ваш номер телефона через кнопку ниже:", replyMarkup: BotReplyKeyboards.RequestPhoneKeyboard());
            }
        }

        private static async Task HandleAuthorizedUserAsync(ITelegramBotClient botClient, Message message, long chatId)
        {
            switch ( message.Text )
            {
                case "👤 Профиль":
                    try
                    {
                        var profile = await UserService.GetUserProfileByChatIdAsync(chatId);

                        string profileMessage = $"👤 *Профиль*\n\n" +
                                                $"*ФИО:* {profile.FullName}\n" +
                                                $"*Телефон:* {profile.PhoneNumber}\n" +
                                                $"*Прогресс обучения:* {profile.UserProgressInPercent}%";

                        await botClient.SendMessage(chatId, profileMessage, parseMode: ParseMode.Markdown, replyMarkup: BotReplyKeyboards.GetMainMenu());
                    }
                    catch ( Exception ex )
                    {
                        Console.WriteLine($"Ошибка при получении профиля: {ex.Message}");
                        await botClient.SendMessage(chatId, "⚠️ Ошибка при получении профиля. Попробуйте позже.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                    }
                    break;

                case "💡 Темы":
                    var titles = await TestService.GetTestTitlesAsync();

                    if ( titles.Count == 0 )
                    {
                        await botClient.SendMessage(chatId, "❗ Тем пока нет.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                    }
                    else
                    {
                        var keyboard = BotReplyKeyboards.GetTopicsKeyboard(titles);
                        await botClient.SendMessage(chatId, "📚 Выберите тему:", replyMarkup: keyboard);
                    }
                    break;

                case "🛠 Техподдержка":
                    await botClient.SendMessage(chatId, "Свяжитесь с [@SupportUsername](https://t.me/SupportUsername)", parseMode: ParseMode.Markdown, replyMarkup: BotReplyKeyboards.GetMainMenu());
                    break;

                case "/menu":
                    await botClient.SendMessage(chatId, "📋 Главное меню:", replyMarkup: BotReplyKeyboards.GetMainMenu());
                    break;

                default:
                    await botClient.SendMessage(chatId, "🤔 Я не понимаю. Выберите действие:", replyMarkup: BotReplyKeyboards.GetMainMenu());
                    break;
            }
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
