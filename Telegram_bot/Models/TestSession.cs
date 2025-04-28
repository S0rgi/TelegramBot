using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramMenuBot.Models;

namespace Telegram_bot.Models
{
    public class TestSession
    {
        public List<QuestionModel> Questions { get; set; }
        public int CurrentQuestionIndex { get; set; }
    }

}
