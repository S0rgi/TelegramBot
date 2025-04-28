using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramMenuBot.Services;

public static class QuizHandler
{
    public static async Task HandleTopicSelectedAsync(ITelegramBotClient botClient, Update update)
    {
        var chatId = update.Message.Chat.Id;
        var topicTitle = update.Message.Text;

        // 1. Получить список вопросов по теме
        var questions = await TestService.GetQuestionsByTopicAsync(topicTitle);

        if ( questions.Count == 0 )
        {
            await botClient.SendMessage(chatId, "Не удалось найти вопросы по выбранной теме.");
            return;
        }

        // 2. Начать квиз
        await QuizSessionService.StartQuizAsync(chatId, questions);

        // 3. Отправить первый вопрос
        await SendCurrentQuestionAsync(botClient, chatId);
    }

    public static async Task SendCurrentQuestionAsync(ITelegramBotClient botClient, long chatId)
    {
        var session = QuizSessionService.GetSession(chatId);
        if ( session == null )
        {
            await botClient.SendMessage(chatId, "Квиз не найден. Пожалуйста, выберите тему сначала.");
            return;
        }

        if ( session.CurrentIndex >= session.Questions.Count )
        {
            await botClient.SendMessage(chatId, "Квиз завершён! Молодец!");
            QuizSessionService.EndQuiz(chatId);
            return;
        }

        var question = session.Questions[session.CurrentIndex];

        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { question.IssueChoice1 },
            new KeyboardButton[] { question.IssueChoice2 },
            new KeyboardButton[] { question.IssueChoice3 },
            new KeyboardButton[] { question.IssueChoice4 },
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await botClient.SendMessage(
            chatId: chatId,
            text: question.Issue,
            replyMarkup: keyboard
        );
    }
    public static async Task HandleAnswerAsync(ITelegramBotClient botClient, Update update)
    {
        var chatId = update.Message.Chat.Id;
        var userAnswer = update.Message.Text;

        var session = QuizSessionService.GetSession(chatId);
        if ( session == null )
        {
            await botClient.SendMessage(chatId, "Квиз не найден. Пожалуйста, выберите тему сначала.");
            return;
        }

        if ( session.CurrentIndex >= session.Questions.Count )
        {
            await botClient.SendMessage(chatId, "Квиз уже завершён!");
            QuizSessionService.EndQuiz(chatId);
            return;
        }

        var currentQuestion = session.Questions[session.CurrentIndex];

        if ( userAnswer == currentQuestion.CorrectAnswer )
        {
            await botClient.SendMessage(chatId, "✅ Правильно!");
        }
        else
        {
            await botClient.SendMessage(chatId, $"❌ Неправильно. Правильный ответ: {currentQuestion.CorrectAnswer}");
        }

        // Перейти к следующему вопросу
        QuizSessionService.NextQuestion(chatId);

        // Отправить следующий вопрос
        await SendCurrentQuestionAsync(botClient, chatId);
    }
}
