using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramMenuBot.Services;
using TelegramMenuBot.Bot;
using TelegramMenuBot.Utilities;
using Telegram_bot.Models;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Concurrent;
using System.Text.Json;
using Telegram_bot.Utilities;
using Telegram_bot.TestBot;
using Telegram_bot.Bot.Handlers;
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

            // Если пользователь в процессе теста
            if ( Sessions.UserTestSessions.TryGetValue(chatId, out var sessionInProgress) )
            {
                await Test.HandleTestAnswerAsync(botClient, message, chatId, sessionInProgress);
                return;
            }

            if ( message.Text == "/start" )
            {
                bool userExists = await UserService.CheckIfUserExistsByChatIdAsync(chatId);

                if ( userExists )
                {
                    await botClient.SendMessage(chatId, "📋 Главное меню:", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }
                else
                {
                    await botClient.SendMessage(chatId, "Привет! Рады приветствовать на нашей обучающей программе тимлид!");
                    await botClient.SendMessage(chatId, "Введи свое ФИО.");
                }
                return;
            }

            bool isRegistered = await UserService.CheckIfUserExistsByChatIdAsync(chatId);

            if ( !isRegistered )
            {
                await HandleRegistrationAsync(botClient, message, chatId);
                return;
            }

            var titles = await TestService.GetTestTitlesAsync();


            if ( titles.Contains(message.Text) )
            {
                if ( Sessions.PendingTestTopics.TryGetValue(chatId, out var pendingType) )
                {
                    if ( pendingType == "materials" )
                    {
                        await Test.SendMaterials(botClient, chatId, message.Text);
                    }
                    else if ( pendingType == "tests" )
                    {
                        await Test.StartTest(botClient, chatId, message.Text);
                    }

                    Sessions.PendingTestTopics.TryRemove(chatId, out _);
                }
                else
                {
                    await botClient.SendMessage(chatId, "⚠️ Пожалуйста выберите сначала материалы или тесты.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }

                return;

            }
            if ( message.Text != null && message.Text.StartsWith("Перейти к выполнению теста") )
            {
                if ( Sessions.PendingTestTopics.TryGetValue(chatId, out var topicTitle) )
                {
                    var questions = await TestService.GetTopicQuestionsAsync(topicTitle);

                    if ( questions.Count == 0 )
                    {
                        await botClient.SendMessage(chatId, "❗ По этой теме пока нет вопросов.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                    }
                    else
                    {
                        var testSession = new TestSession
                        {
                            Questions = questions,
                            CurrentQuestionIndex = 0,
                            CorrectAnswersCount = 0,
                            TopicTitle = topicTitle
                        };

                        Sessions.UserTestSessions[chatId] = testSession;
                        Sessions.PendingTestTopics.TryRemove(chatId, out _);

                        await Test.SendCurrentQuestion(botClient, chatId, testSession);
                    }
                }
                else
                {
                    await botClient.SendMessage(chatId, "⚠️ Тема не найдена. Пожалуйста, выберите тему снова.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }

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
                    await botClient.SendMessage(chatId, "✅ Супер! Ты успешно авторизован.", replyMarkup: BotReplyKeyboards.GetMainMenu());
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
                await botClient.SendMessage(chatId, "📱 Теперь нужно подтвердить твой номер телефона.", replyMarkup: BotReplyKeyboards.RequestPhoneKeyboard());
            }
        }

        private static async Task HandleAuthorizedUserAsync(ITelegramBotClient botClient, Message message, long chatId)
        {
            if ( await MenuesHandlers.HandleMainMenu(botClient, message, chatId) )
                return;

            if ( await MenuesHandlers.HandlePopularQuestions(botClient, message, chatId) )
                return;

            if ( await TestHandlers.HandleMaterialsAsync(botClient, message, chatId) )
                return;

            if ( await TestHandlers.HandleTestsAsync(botClient, message, chatId) )
                return;

            switch ( message.Text )
            {
                case "Вернуться в Главное меню":
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
