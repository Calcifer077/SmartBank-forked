namespace SmartBank.API.Helpers
{
    public static class AccountNumberGenerator
    {
        // Generates format: SB-20240001234567
        public static string Generate()
        {
            var year = DateTime.UtcNow.Year;
            var random = new Random();
            var digits = random.NextInt64(1000000000L, 9999999999L);
            return $"SB-{year}{digits}";
        }
    }
}