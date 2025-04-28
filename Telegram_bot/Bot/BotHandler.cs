using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramMenuBot.Services;
using TelegramMenuBot.Bot;
using TelegramMenuBot.Utilities;
using Telegram_bot.Models;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Concurrent;

namespace TelegramMenuBot.Bot
{
    public static class BotHandlers
    {
        private static ConcurrentDictionary<long, TestSession> UserTestSessions = new();

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if ( update.Type != UpdateType.Message || update.Message == null )
                return;

            var message = update.Message;
            var chatId = message.Chat.Id;
            // Проверка - пользователь в процессе теста?
            if ( UserTestSessions.TryGetValue(chatId, out var sessionInProgress) )
            {
                var currentQuestion = sessionInProgress.Questions[sessionInProgress.CurrentQuestionIndex];

                // Здесь можно проверить правильность ответа:
                if ( message.Text.Trim() == currentQuestion.CorrectAnswer.Trim() )
                {
                    await botClient.SendMessage(chatId, "✅ Верно!");
                }
                else
                {
                    await botClient.SendMessage(chatId, $"❌ Неверно. Правильный ответ: {currentQuestion.CorrectAnswer}");
                }

                sessionInProgress.CurrentQuestionIndex++;

                if ( sessionInProgress.CurrentQuestionIndex < sessionInProgress.Questions.Count )
                {
                    await SendCurrentQuestion(botClient, chatId, sessionInProgress);
                }
                else
                {
                    UserTestSessions.TryRemove(chatId, out _); // Завершили тест
                    await botClient.SendMessage(chatId, "🏁 Тест завершен!", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }
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
            var titles = await TestService.GetTestTitlesAsync();

            if ( titles.Contains(message.Text) )
            {
                var questions = await TestService.GetTopicQuestionsAsync(message.Text);

                if ( questions.Count == 0 )
                {
                    await botClient.SendMessage(chatId, "❗ По этой теме пока нет вопросов.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }
                else
                {
                    var testSession = new TestSession
                    {
                        Questions = questions,
                        CurrentQuestionIndex = 0
                    };

                    UserTestSessions[chatId] = testSession;

                    await SendCurrentQuestion(botClient, chatId, testSession);
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
                    {
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
                    }


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
        private static async Task SendCurrentQuestion(ITelegramBotClient botClient, long chatId, TestSession session)
        {
            var currentQuestion = session.Questions[session.CurrentQuestionIndex];

            string questionText = $"*{currentQuestion.Issue}*";

            // Собираем только непустые варианты
            var choices = new List<string>();
            if ( !string.IsNullOrWhiteSpace(currentQuestion.IssueChoice1) ) choices.Add(currentQuestion.IssueChoice1);
            if ( !string.IsNullOrWhiteSpace(currentQuestion.IssueChoice2) ) choices.Add(currentQuestion.IssueChoice2);
            if ( !string.IsNullOrWhiteSpace(currentQuestion.IssueChoice3) ) choices.Add(currentQuestion.IssueChoice3);
            if ( !string.IsNullOrWhiteSpace(currentQuestion.IssueChoice4) ) choices.Add(currentQuestion.IssueChoice4);

            var keyboardButtons = choices
                .Select(choice => new[] { new KeyboardButton(choice) })
                .ToArray();

            var keyboard = new ReplyKeyboardMarkup(keyboardButtons)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await botClient.SendMessage(chatId, questionText, parseMode: ParseMode.Markdown, replyMarkup: keyboard);
        }


    }
}
