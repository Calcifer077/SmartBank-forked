namespace SmartBank.MVC.Helpers
{
    public static class SessionHelper
    {
        private const string TokenKey = "JwtToken";
        private const string UserNameKey = "UserName";
        private const string UserEmailKey = "UserEmail";
        private const string UserRoleKey = "UserRole";
        private const string UserIdKey = "UserId";

        public static void SetUserSession(ISession session,
            string token, string name, string email, string role, int userId)
        {
            session.SetString(TokenKey, token);
            session.SetString(UserNameKey, name);
            session.SetString(UserEmailKey, email);
            session.SetString(UserRoleKey, role);
            session.SetInt32(UserIdKey, userId);
        }

        public static void ClearSession(ISession session) =>
            session.Clear();

        public static string? GetToken(ISession session) =>
            session.GetString(TokenKey);

        public static string GetUserName(ISession session) =>
            session.GetString(UserNameKey) ?? "User";

        public static string GetUserEmail(ISession session) =>
            session.GetString(UserEmailKey) ?? string.Empty;

        public static string GetUserRole(ISession session) =>
            session.GetString(UserRoleKey) ?? string.Empty;

        public static int GetUserId(ISession session) =>
            session.GetInt32(UserIdKey) ?? 0;

        public static bool IsLoggedIn(ISession session) =>
            !string.IsNullOrEmpty(session.GetString(TokenKey));
    }
}