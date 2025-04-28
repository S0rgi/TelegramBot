using TelegramMenuBot.Models;
namespace TelegramMenuBot.Services
{
    public static class QuizSessionService
    {
        private static readonly Dictionary<long, QuizSession> _sessions = new();

        public static Task StartQuizAsync(long chatId, List<QuestionModel> questions)
        {
            _sessions[chatId] = new QuizSession { Questions = questions, CurrentIndex = 0 };
            return Task.CompletedTask;
        }

        public static QuizSession? GetSession(long chatId)
        {
            _sessions.TryGetValue(chatId, out var session);
            return session;
        }

        public static void NextQuestion(long chatId)
        {
            if ( _sessions.TryGetValue(chatId, out var session) )
            {
                session.CurrentIndex++;
            }
        }

        public static void EndQuiz(long chatId)
        {
            _sessions.Remove(chatId);
        }
    }

    public class QuizSession
    {
        public List<QuestionModel> Questions { get; set; } = new();
        public int CurrentIndex { get; set; } = 0;
    }
}
