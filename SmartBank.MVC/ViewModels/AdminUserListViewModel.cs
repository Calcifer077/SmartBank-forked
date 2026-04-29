namespace SmartBank.MVC.ViewModels
{
    public class AdminUserItemViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string KycStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccountCount { get; set; }
    }

    public class AdminUserListViewModel
    {
        public List<AdminUserItemViewModel> Users { get; set; } = new();
    }
}