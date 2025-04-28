using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramMenuBot.Bot
{
    public static class BotReplyKeyboards
    {
        public static ReplyKeyboardMarkup RequestPhoneKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("📞 Подтвердить номер телефона") { RequestContact = true } }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetMainMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("👤 Профиль") },
                new[] {new KeyboardButton("💡 Начать проходить курс сейчас") },
                new[] {new KeyboardButton("Напомнить о курсе через 12 часов") },
                new[] { new KeyboardButton("Популярные вопросы") }
            })
            {
                ResizeKeyboard = true
            };
        }
        public static ReplyKeyboardMarkup GetPopularQuestions()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("Расскажи про программу курса") },
                new[] { new KeyboardButton("Сколько по времени будет продолжительность курса?") },
                new[] { new KeyboardButton("Останутся ли у меня материалы после прохождения курса?") },
                new[] { new KeyboardButton("🛠 Обратиться в техподдержку.") },
                new[] { new KeyboardButton("Вернуться в Главное меню") }
            })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetTopicsKeyboard(List<string> titles)
        {
            return new ReplyKeyboardMarkup(
                titles.Select(title => new[] { new KeyboardButton(title) }).ToArray()
            )
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };
        }
        public static ReplyKeyboardMarkup GetPassTestButton(string topicTitle)
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton($"▶️ Пройти тест по теме \"{topicTitle}\"") },
                new[] { new KeyboardButton("Вернуться в Главное меню") }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }
        public static ReplyKeyboardMarkup GetCourseChoiceMenu()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("📚 Материалы"), new KeyboardButton("📝 Тесты") },
                new[] { new KeyboardButton("Вернуться в Главное меню") }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }

    }
}
