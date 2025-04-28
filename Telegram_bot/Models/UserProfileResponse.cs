namespace TelegramMenuBot.Models
{
    public class UserProfileResponse
    {
        public long ChatId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public int UserProgressInPercent { get; set; }
    }
}
