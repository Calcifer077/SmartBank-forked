namespace SmartBank.API.DTOs.Admin
{
    public class FreezeAccountDto
    {
        public int AccountId { get; set; }
        public string Action { get; set; } = string.Empty; // Freeze, Unfreeze
    }
}