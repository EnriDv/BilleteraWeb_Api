using Domain.Entities; 
using System.ComponentModel.DataAnnotations;

namespace InterfaceAdapters.DTO.User;

public record AcceptTransactionRequestDto(
    [Required]
    Guid TransactionId
);