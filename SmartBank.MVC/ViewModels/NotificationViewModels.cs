namespace SmartBank.MVC.ViewModels
{
    public class NotificationItemViewModel
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class NotificationListViewModel
    {
        public List<NotificationItemViewModel> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}
