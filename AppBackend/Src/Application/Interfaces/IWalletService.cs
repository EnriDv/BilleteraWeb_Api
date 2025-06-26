using Application.DTO;

namespace Application.Interfaces;

public interface IWalletService
{
    Task<WalletDto> GetWalletByUserIdAsync(int userId);
    Task<TransactionDto> SendMoneyAsync(int senderUserId, string recipientPhoneNumber, decimal amount, string? description);
    Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(int userId);
    Task AcceptTransactionAsync(int userId, Guid transactionId);
}