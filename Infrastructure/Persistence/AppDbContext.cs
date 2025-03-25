using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    // Infrastructure/Persistence/AppDbContext.cs
    public class AppDbContext : DbContext
    {
        public DbSet<PaymentToken> PaymentTokens { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }

    // Domain/Entities/PaymentToken.cs
    public class PaymentToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public bool IsUsed { get; set; }
        public DateTime Expiration { get; set; }
    }
}
