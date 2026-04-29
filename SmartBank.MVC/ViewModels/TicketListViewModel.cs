namespace SmartBank.MVC.ViewModels
{
    public class TicketItemViewModel
    {
        public int TicketId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class TicketListViewModel
    {
        public List<TicketItemViewModel> Tickets { get; set; } = new();
    }
}