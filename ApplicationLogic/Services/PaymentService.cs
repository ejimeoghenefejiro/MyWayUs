using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLogic.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLogic.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return await _context.PaymentTokens
                .AnyAsync(t => t.Token == token
                    && !t.IsUsed
                    && t.Expiration > DateTime.UtcNow);
        }
    }
}
