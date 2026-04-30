using Microsoft.EntityFrameworkCore;
using SmartBank.Data;

namespace SmartBank.Tests.Helpers
{
    public static class DbContextHelper
    {
        public static SmartBankDbContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SmartBankDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new SmartBankDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}