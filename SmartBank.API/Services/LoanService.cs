using Microsoft.EntityFrameworkCore;
using SmartBank.API.DTOs.Loans;
using SmartBank.Data;
using SmartBank.Models.Entities;

namespace SmartBank.API.Services
{
    public class LoanService : ILoanService
    {
        private readonly SmartBankDbContext _db;
        private readonly ILogger<LoanService> _logger;

        public LoanService(SmartBankDbContext db, ILogger<LoanService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<LoanResponseDto> ApplyAsync(int userId, LoanApplyDto dto)
        {
            // Check user exists
            var user = await _db.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            // Block if pending loan already exists
            bool hasPending = await _db.Loans
                .AnyAsync(l => l.UserId == userId && l.Status == "Pending");

            if (hasPending)
                throw new InvalidOperationException(
                    "You already have a pending loan application. " +
                    "Wait for it to be reviewed before applying again.");

            var loan = new Loan
            {
                UserId = userId,
                Amount = dto.Amount,
                TermMonths = dto.TermMonths,
                Purpose = dto.Purpose,
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            };

            _db.Loans.Add(loan);
            await _db.SaveChangesAsync();

            // Notify user
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = "Loan Application Received",
                Message = $"Your loan application of ₹{dto.Amount:N2} " +
                          $"for {dto.TermMonths} months has been submitted " +
                          $"and is under review.",
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Loan applied: UserId {UserId}, Amount ₹{Amount}", userId, dto.Amount);

            return MapToDto(loan, user.FullName, user.Email);
        }

        public async Task<List<LoanResponseDto>> GetMyLoansAsync(int userId)
        {
            return await _db.Loans
                .Include(l => l.User)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.AppliedAt)
                .Select(l => MapToDto(l, l.User.FullName, l.User.Email))
                .ToListAsync();
        }

        public async Task<LoanResponseDto> GetDetailAsync(int loanId, int userId)
        {
            var loan = await _db.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LoanId == loanId)
                ?? throw new KeyNotFoundException("Loan not found.");

            if (loan.UserId != userId)
                throw new UnauthorizedAccessException("Access denied.");

            return MapToDto(loan, loan.User.FullName, loan.User.Email);
        }

        public async Task<List<LoanResponseDto>> GetAllLoansAsync()
        {
            return await _db.Loans
                .Include(l => l.User)
                .OrderByDescending(l => l.AppliedAt)
                .Select(l => MapToDto(l, l.User.FullName, l.User.Email))
                .ToListAsync();
        }

        public async Task<LoanResponseDto> ReviewLoanAsync(LoanReviewDto dto)
        {
            var allowedDecisions = new[] { "Approved", "Rejected" };

            if (!allowedDecisions.Contains(dto.Decision))
                throw new InvalidOperationException("Decision must be Approved or Rejected.");

            var loan = await _db.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LoanId == dto.LoanId)
                ?? throw new KeyNotFoundException("Loan not found.");

            if (loan.Status != "Pending")
                throw new InvalidOperationException(
                    $"Loan is already {loan.Status}. Cannot review again.");

            loan.Status = dto.Decision;
            loan.ReviewedAt = DateTime.UtcNow;

            // Notify applicant
            _db.Notifications.Add(new Notification
            {
                UserId = loan.UserId,
                Title = $"Loan {dto.Decision}",
                Message = dto.Decision == "Approved"
                    ? $"Congratulations! Your loan of ₹{loan.Amount:N2} " +
                      $"has been approved."
                    : $"Your loan application of ₹{loan.Amount:N2} " +
                      $"has been rejected. Contact support for details.",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Loan {LoanId} {Decision}", dto.LoanId, dto.Decision);

            return MapToDto(loan, loan.User.FullName, loan.User.Email);
        }

        // ── Mapper ────────────────────────────────────────────────────
        private static LoanResponseDto MapToDto(
            Loan loan, string name, string email) => new()
        {
            LoanId = loan.LoanId,
            Amount = loan.Amount,
            TermMonths = loan.TermMonths,
            Purpose = loan.Purpose,
            Status = loan.Status,
            AppliedAt = loan.AppliedAt,
            ReviewedAt = loan.ReviewedAt,
            ApplicantName = name,
            ApplicantEmail = email
        };
    }
}