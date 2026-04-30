namespace SmartBank.API.DTOs.Admin
{
    public class ChangeRoleDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
