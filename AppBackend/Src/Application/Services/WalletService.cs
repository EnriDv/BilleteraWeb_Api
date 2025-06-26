using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repository;

namespace Application.Services;

public class WalletService : IWalletService
{
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<WalletDto> GetWalletByUserIdAsync(int userId)
    {
        var wallets = await _unitOfWork.Wallets.FindAsync(w => w.UserId == userId);
        var wallet = wallets.FirstOrDefault();

        if (wallet == null)
        {
            throw new KeyNotFoundException("No se encontró una billetera para el usuario especificado.");
        }
        return new WalletDto(
            wallet.Id,
            wallet.Balance,
            wallet.Currency
        );
    }
    public async Task<TransactionDto> SendMoneyAsync(int senderUserId, string recipientPhoneNumber, decimal amount, string? description)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("El monto de la transferencia debe ser positivo.");
        }
        var senderWallet = (await _unitOfWork.Wallets.FindAsync(w => w.UserId == senderUserId)).FirstOrDefault();
        var recipientUser = await _unitOfWork.Users.GetByPhoneNumberAsync(recipientPhoneNumber);
        if (recipientUser == null)
        {
            throw new KeyNotFoundException("El número de teléfono del destinatario no está registrado.");
        }
        
        var recipientWallet = (await _unitOfWork.Wallets.FindAsync(w => w.UserId == recipientUser.Id)).FirstOrDefault();
        if (senderWallet == null || recipientWallet == null)
        {
            throw new Exception("No se pudo encontrar la billetera de origen o destino.");
        }
        if (senderWallet.Id == recipientWallet.Id)
        {
            throw new InvalidOperationException("No puedes enviarte dinero a ti mismo.");
        }
        if (senderWallet.Balance < amount)
        {
            throw new InvalidOperationException("Saldo insuficiente para realizar la transferencia.");
        }

        senderWallet.Balance -= amount;
        recipientWallet.Balance += amount;

        _unitOfWork.Wallets.Update(senderWallet);
        _unitOfWork.Wallets.Update(recipientWallet);

        Transaction transaction;
        if(amount >= 1000){
            transaction = new Transaction
            {
                SourceWalletId = senderWallet.Id,
                DestinationWalletId = recipientWallet.Id,
                Amount = amount,
                Type = TransactionType.P2P_TRANSFER,
                Status = TransactionStatus.PENDING,
                Description = description,
                Timestamp = DateTime.UtcNow
            };
        }
        else {
            transaction = new Transaction
            {
                SourceWalletId = senderWallet.Id,
                DestinationWalletId = recipientWallet.Id,
                Amount = amount,
                Type = TransactionType.P2P_TRANSFER,
                Status = TransactionStatus.COMPLETED,
                Description = description,
                Timestamp = DateTime.UtcNow
            };
        }
        
        await _unitOfWork.Transactions.AddAsync(transaction);

        await _unitOfWork.SaveChangesAsync();
        return new TransactionDto(
            transaction.Id,
            transaction.Type.ToString(),
            transaction.Status.ToString(),
            transaction.Amount,
            transaction.Description,
            transaction.Timestamp,
            transaction.SourceWalletId.Value,
            transaction.DestinationWalletId.Value
        );
    }


    public async Task AcceptTransactionAsync(int userId, Guid transactionId)
    {
        var transactions = await _unitOfWork.Transactions.FindAsync(t => t.Id == transactionId);
        var tx = transactions.FirstOrDefault();

        if (tx == null)
            throw new KeyNotFoundException("La transacción no existe.");

        var recipientWallets = await _unitOfWork.Wallets.FindAsync(w => w.Id == tx.DestinationWalletId);
        var wallet = recipientWallets.FirstOrDefault();

        if (wallet == null || wallet.UserId != userId)
            throw new UnauthorizedAccessException("Ha ocurrido un error al aceptar la transacción.");

        if (tx.Status != TransactionStatus.PENDING)
            throw new InvalidOperationException("La transacción no está pendiente.");

        tx.Status = TransactionStatus.COMPLETED;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(int userId)
    {
        var wallet = (await _unitOfWork.Wallets.FindAsync(w => w.UserId == userId)).FirstOrDefault();
        if (wallet == null)
        {
            throw new KeyNotFoundException("No se encontró una billetera para el usuario.");
        }
        var transactions = await _unitOfWork.Transactions.FindAsync(
            t => t.SourceWalletId == wallet.Id || t.DestinationWalletId == wallet.Id
        );
        var orderedTransactions = transactions.OrderByDescending(t => t.Timestamp);

        return orderedTransactions.Select(t => new TransactionDto(
            t.Id,
            t.Type.ToString(),
            t.Status.ToString(),
            t.Amount,
            t.Description,
            t.Timestamp,
            t.SourceWalletId,
            t.DestinationWalletId
        ));
    }
}