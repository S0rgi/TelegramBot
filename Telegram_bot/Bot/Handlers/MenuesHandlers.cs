using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramMenuBot.Bot;
using TelegramMenuBot.Services;

namespace Telegram_bot.Bot.Handlers
{
    public static class MenuesHandlers
    {
        public static async Task<bool> HandlePopularQuestions(ITelegramBotClient botClient, Message message, long chatId)
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

        public static async Task<bool> HandleMainMenu(ITelegramBotClient botClient, Message message, long chatId)
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
    }
}
