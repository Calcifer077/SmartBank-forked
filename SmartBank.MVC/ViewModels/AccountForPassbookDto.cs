namespace SmartBank.MVC.ViewModels
{
    public class AccountForPassbookDto
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Status { get; set; } = "Active";
    }
}
