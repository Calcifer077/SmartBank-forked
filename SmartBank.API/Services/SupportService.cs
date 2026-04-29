using Microsoft.EntityFrameworkCore;
using SmartBank.API.DTOs.Support;
using SmartBank.Data;
using SmartBank.Models.Entities;

namespace SmartBank.API.Services
{
    public class SupportService : ISupportService
    {
        private readonly SmartBankDbContext _db;
        private readonly ILogger<SupportService> _logger;

        public SupportService(SmartBankDbContext db, ILogger<SupportService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<TicketResponseDto> CreateTicketAsync(int userId, TicketCreateDto dto)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            var ticket = new SupportTicket
            {
                UserId = userId,
                Subject = dto.Subject,
                Description = dto.Description,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _db.SupportTickets.Add(ticket);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Support ticket created: TicketId {TicketId} by UserId {UserId}",
                ticket.TicketId, userId);

            return MapToDto(ticket, user.FullName);
        }

        public async Task<List<TicketResponseDto>> GetMyTicketsAsync(int userId)
        {
            return await _db.SupportTickets
                .Include(t => t.User)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => MapToDto(t, t.User.FullName))
                .ToListAsync();
        }

        public async Task<List<TicketResponseDto>> GetAllTicketsAsync()
        {
            return await _db.SupportTickets
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => MapToDto(t, t.User.FullName))
                .ToListAsync();
        }

        public async Task<TicketResponseDto> ResolveTicketAsync(int ticketId)
        {
            var ticket = await _db.SupportTickets
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId)
                ?? throw new KeyNotFoundException("Ticket not found.");

            if (ticket.Status == "Resolved")
                throw new InvalidOperationException("Ticket is already resolved.");

            ticket.Status = "Resolved";
            ticket.ResolvedAt = DateTime.UtcNow;

            // Notify user
            _db.Notifications.Add(new Notification
            {
                UserId = ticket.UserId,
                Title = "Support Ticket Resolved",
                Message = $"Your ticket '{ticket.Subject}' has been resolved.",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketId} resolved", ticketId);

            return MapToDto(ticket, ticket.User.FullName);
        }

        // ── Mapper ────────────────────────────────────────────────────
        private static TicketResponseDto MapToDto(
            SupportTicket ticket, string name) => new()
        {
            TicketId = ticket.TicketId,
            Subject = ticket.Subject,
            Description = ticket.Description,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = ticket.ResolvedAt,
            ApplicantName = name
        };
    }
}