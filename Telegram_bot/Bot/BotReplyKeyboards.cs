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
                new[] { new KeyboardButton("💡 Темы") },
                new[] { new KeyboardButton("🛠 Техподдержка") }
            })
            {
                ResizeKeyboard = true
            };
        }


        public static ReplyKeyboardMarkup GetTestKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("40"), new KeyboardButton("42"), new KeyboardButton("45") }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
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
    }
}
