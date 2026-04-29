using SmartBank.API.DTOs.Support;

namespace SmartBank.API.Services
{
    public interface ISupportService
    {
        Task<TicketResponseDto> CreateTicketAsync(int userId, TicketCreateDto dto);
        Task<List<TicketResponseDto>> GetMyTicketsAsync(int userId);
        Task<List<TicketResponseDto>> GetAllTicketsAsync();       // Admin
        Task<TicketResponseDto> ResolveTicketAsync(int ticketId); // Admin
    }
}