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

namespace TelegramMenuBot.Bot
{
    public static class BotHandlers
    {
        private static ConcurrentDictionary<long, TestSession> UserTestSessions = new();
        private static readonly string[] AnswerLetters = { "a", "b", "c", "d" };
        private static ConcurrentDictionary<long, string> PendingTestTopics = new();

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if ( update.Type != UpdateType.Message || update.Message == null )
                return;

            var message = update.Message;
            var chatId = message.Chat.Id;

            // Если пользователь в процессе теста
            if ( UserTestSessions.TryGetValue(chatId, out var sessionInProgress) )
            {
                await HandleTestAnswerAsync(botClient, message, chatId, sessionInProgress);
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
                if ( PendingTestTopics.TryGetValue(chatId, out var pendingType) )
                {
                    if ( pendingType == "materials" )
                    {
                        await SendMaterials(botClient, chatId, message.Text);
                    }
                    else if ( pendingType == "tests" )
                    {
                        await StartTest(botClient, chatId, message.Text);
                    }

                    PendingTestTopics.TryRemove(chatId, out _);
                }
                else
                {
                    await botClient.SendMessage(chatId, "⚠️ Пожалуйста выберите сначала материалы или тесты.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }

                return;

            }
            if ( message.Text != null && message.Text.StartsWith("Перейти к выполнению теста") )
            {
                if ( PendingTestTopics.TryGetValue(chatId, out var topicTitle) )
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

                        UserTestSessions[chatId] = testSession;
                        PendingTestTopics.TryRemove(chatId, out _);

                        await SendCurrentQuestion(botClient, chatId, testSession);
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

        private static async Task HandleTestAnswerAsync(ITelegramBotClient botClient, Message message, long chatId, TestSession session)
        {
            var currentQuestion = session.Questions[session.CurrentQuestionIndex];

            // Вытащить выбранную букву ответа
            var selectedLetter = message.Text?.Split(':')[0].Trim().ToLower();

            if ( string.IsNullOrEmpty(selectedLetter) )
            {
                await botClient.SendMessage(chatId, "⚠️ Неверный формат ответа. Пожалуйста, выберите вариант ответа на клавиатуре.");
                return;
            }

            var correctAnswer = currentQuestion.CorrectAnswer.Trim().ToLower();

            if ( selectedLetter == correctAnswer )
            {
                session.CorrectAnswersCount++;
                await botClient.SendMessage(chatId, "✅ Ты молодец! Это правильный ответ");
            }
            else
            {
                await botClient.SendMessage(chatId, "К сожалению, ответ неверный. Попробуй пройти материал еще раз, у тебя все получится!");
            }

            session.CurrentQuestionIndex++;
            if ( session.CurrentQuestionIndex < session.Questions.Count )
            {
                await SendCurrentQuestion(botClient, chatId, session);
            }
            else
            {
                await FinishTestAsync(botClient, chatId, session);
            }
        }

        private static async Task FinishTestAsync(ITelegramBotClient botClient, long chatId, TestSession session)
        {
            int totalQuestions = session.Questions.Count;
            int correctAnswers = session.CorrectAnswersCount;
            double percentCorrect = (double)correctAnswers / totalQuestions * 100;

            UserTestSessions.TryRemove(chatId, out _); // Завершили тест
            if ( string.IsNullOrEmpty(session.TopicTitle) )
            {
                Console.WriteLine("❌ Ошибка: не найдено название темы в сессии");
                await botClient.SendMessage(chatId, "⚠️ Произошла ошибка при обработке теста");
                return;
            }
            if ( percentCorrect > 66 )
            {
                try
                {
                    string topicEncoded = Uri.EscapeDataString(session.TopicTitle);

                    string requestUrl = $"http://localhost:5135/api/v1/User/{chatId},{topicEncoded}";

                    using var httpClient = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Patch, requestUrl);
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));

                    var response = await httpClient.SendAsync(request);

                    if ( response.IsSuccessStatusCode )
                    {
                        Console.WriteLine("✅ Прогресс успешно обновлен.");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Ошибка обновления прогресса: {response.StatusCode}");
                    }
                }
                catch ( Exception ex )
                {
                    Console.WriteLine($"❌ Ошибка при отправке запроса на обновление прогресса: {ex.Message}");
                }
            }

            string resultMessage = $"🏁 *Тест завершен!*\n\n" +
                                   $"*Правильных ответов:* {correctAnswers} из {totalQuestions}\n" +
                                   $"*Процент правильных ответов:* {percentCorrect:F1}%\n";
            if ( percentCorrect > 66 )
                resultMessage += "Тема сдана!";
            else
                resultMessage += "Тема не сдана.";
            await botClient.SendMessage(chatId, resultMessage, parseMode: ParseMode.Markdown, replyMarkup: BotReplyKeyboards.GetMainMenu());
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
            if ( await HandleMainMenu(botClient, message, chatId) )
                return;

            if ( await HandlePopularQuestions(botClient, message, chatId) )
                return;
            if ( message.Text == "📚 Материалы" )
            {
                var titles = await TestService.GetTestTitlesAsync();
                if ( titles.Count == 0 )
                {
                    await botClient.SendMessage(chatId, "❗ Тем пока нет.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }
                else
                {
                    var keyboard = BotReplyKeyboards.GetTopicsKeyboard(titles);
                    await botClient.SendMessage(chatId, "📚 Выберите тему для изучения материалов:", replyMarkup: keyboard);

                    // Чтобы знать что человек выбрал материалы
                    PendingTestTopics[chatId] = "materials";
                }
                return;
            }

            if ( message.Text == "📝 Тесты" )
            {
                var titles = await TestService.GetTestTitlesAsync();
                if ( titles.Count == 0 )
                {
                    await botClient.SendMessage(chatId, "❗ Тем пока нет.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }
                else
                {
                    var keyboard = BotReplyKeyboards.GetTopicsKeyboard(titles);
                    await botClient.SendMessage(chatId, "📝 Выберите тему для прохождения теста:", replyMarkup: keyboard);

                    // Чтобы знать что человек выбрал тесты
                    PendingTestTopics[chatId] = "tests";
                }
                return;
            }

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

        private static async Task<bool> HandlePopularQuestions(ITelegramBotClient botClient, Message message, long chatId)
        {
            switch ( message.Text )
            {
                case "🛠 Обратиться в техподдержку.":
                    await botClient.SendMessage(chatId, "Свяжитесь с [@SupportUsername](https://t.me/SupportUsername)", parseMode: ParseMode.Markdown, replyMarkup: BotReplyKeyboards.GetMainMenu());
                    return true;
                case "Расскажи про программу курса":
                    await botClient.SendMessage(chatId, "Это небольшой курс, о всех темах вы можете узнать, нажав в главном меню 'Начать проходить курс сейчас'.");
                    return true;
                case "Сколько по времени будет продолжительность курса?":
                    await botClient.SendMessage(chatId, "Зависит от вашей скорости обучения, в среднем — 7 дней.");
                    return true;
                case "Останутся ли у меня материалы после прохождения курса?":
                    await botClient.SendMessage(chatId, "Конечно, у вас останется полный доступ ко всем материалам.");
                    return true;
                default:
                    return false;
            }
        }

        private static async Task<bool> HandleMainMenu(ITelegramBotClient botClient, Message message, long chatId)
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
                    return true;

                case "Напомнить о курсе через 12 часов":
                    {
                        var reminderTime = DateTime.Now.AddHours(12);
                        var reminderText = $"Хорошо, напомню тебе в {reminderTime:HH:mm}";

                        await botClient.SendMessage(chatId, reminderText);

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromHours(12));
                            await botClient.SendMessage(chatId, "🔔 Напоминаю о курсе! Пора вернуться к обучению.");
                        });
                    }
                    return true;

                case "💡 Начать проходить курс сейчас":
                    await botClient.SendMessage(chatId, "Что хочешь выбрать?", replyMarkup: BotReplyKeyboards.GetCourseChoiceMenu());
                    return true;


                case "Популярные вопросы":
                    await botClient.SendMessage(chatId, "Самые популярные вопросы:", replyMarkup: BotReplyKeyboards.GetPopularQuestions());
                    return true;

                default:
                    return false;
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

            var choices = new List<string>();
            if ( !string.IsNullOrWhiteSpace(currentQuestion.IssueChoice1) ) choices.Add(currentQuestion.IssueChoice1);
            if ( !string.IsNullOrWhiteSpace(currentQuestion.IssueChoice2) ) choices.Add(currentQuestion.IssueChoice2);
            if ( !string.IsNullOrWhiteSpace(currentQuestion.IssueChoice3) ) choices.Add(currentQuestion.IssueChoice3);
            if ( !string.IsNullOrWhiteSpace(currentQuestion.IssueChoice4) ) choices.Add(currentQuestion.IssueChoice4);

            var keyboardButtons = choices
                .Select((choice, index) => new[] { new KeyboardButton($"{AnswerLetters[index]}: {choice}") })
                .ToArray();

            var keyboard = new ReplyKeyboardMarkup(keyboardButtons)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await botClient.SendMessage(chatId, questionText, parseMode: ParseMode.Markdown, replyMarkup: keyboard);
        }
        private static async Task SendMaterials(ITelegramBotClient botClient, long chatId, string topic)
        {
            try
            {
                string topicEncoded = Uri.EscapeDataString(topic);
                string requestUrl = $"http://localhost:5135/api/v1/Education/{topicEncoded}";

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(requestUrl);

                if ( response.IsSuccessStatusCode )
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var responseString = await response.Content.ReadAsStringAsync();
                    var educationResponses = JsonSerializer.Deserialize<List<EducationResponse>>(responseString, options);

                    if ( educationResponses == null || educationResponses.Count == 0 )
                    {
                        await botClient.SendMessage(chatId, "⚠️ Ссылки на материалы не найдены.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                        return;
                    }

                    foreach ( var education in educationResponses )
                    {
                        if ( string.IsNullOrEmpty(education.EducationLink) )
                            continue;

                        string link = education.EducationLink;

                        string infoMessage = $"ℹ️ Информация по теме для подготовки:\n\n" +
                                             $"[Перейти к материалу]({link})";

                        await botClient.SendMessage(chatId, infoMessage, parseMode: ParseMode.Markdown, replyMarkup: BotReplyKeyboards.GetMainMenu());
                    }
                }
                else
                {
                    await botClient.SendMessage(chatId, "⚠️ Не удалось получить информацию по теме.", replyMarkup: BotReplyKeyboards.GetMainMenu());
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"Ошибка при получении материала по теме: {ex.Message}");
                await botClient.SendMessage(chatId, "⚠️ Ошибка при загрузке материала.", replyMarkup: BotReplyKeyboards.GetMainMenu());
            }
        }

        private static async Task StartTest(ITelegramBotClient botClient, long chatId, string topicTitle)
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

                UserTestSessions[chatId] = testSession;

                await SendCurrentQuestion(botClient, chatId, testSession);
            }
        }

    }
}
