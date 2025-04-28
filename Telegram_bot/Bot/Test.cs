using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram_bot.Models;
using Telegram_bot.Utilities;
using TelegramMenuBot.Bot;
using TelegramMenuBot.Services;

namespace Telegram_bot.TestBot
{
    public class Test
    {
        private static readonly string[] AnswerLetters = { "a", "b", "c", "d" };
        public static async Task SendCurrentQuestion(ITelegramBotClient botClient, long chatId, TestSession session)
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
        public static async Task SendMaterials(ITelegramBotClient botClient, long chatId, string topic)
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

        public static async Task StartTest(ITelegramBotClient botClient, long chatId, string topicTitle)
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

                await SendCurrentQuestion(botClient, chatId, testSession);
            }
        }
        public static async Task HandleTestAnswerAsync(ITelegramBotClient botClient, Message message, long chatId, TestSession session)
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

        public static async Task FinishTestAsync(ITelegramBotClient botClient, long chatId, TestSession session)
        {
            int totalQuestions = session.Questions.Count;
            int correctAnswers = session.CorrectAnswersCount;
            double percentCorrect = (double)correctAnswers / totalQuestions * 100;

            Sessions.UserTestSessions.TryRemove(chatId, out _); // Завершили тест
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
    }
}
