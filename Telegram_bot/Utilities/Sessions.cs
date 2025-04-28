using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram_bot.Models;

namespace Telegram_bot.Utilities
{
    public static class Sessions
    {
        public static ConcurrentDictionary<long, TestSession> UserTestSessions = new();
        public static ConcurrentDictionary<long, string> PendingTestTopics = new();
   }
}
