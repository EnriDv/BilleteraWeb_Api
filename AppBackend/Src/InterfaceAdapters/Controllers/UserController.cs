using Application.Interfaces;
using InterfaceAdapters.DTO.Auth;
using InterfaceAdapters.DTO.Transaction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; 
using InterfaceAdapters.DTO.User;

namespace InterfaceAdapters.Controllers;

[ApiController]
[Route("/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IWalletService _walletService;

    public UserController(IUserService userService, IWalletService walletService)
    {
        _userService = userService;
        _walletService = walletService;
    }

    [HttpPut("change-pin")]
    public async Task<IActionResult> ChangePin([FromBody] ChangePinRequestDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue("id"));
            await _userService.ChangePinAsync(userId, request.CurrentPin, request.NewPin);
            return Ok(new { message = "PIN actualizado exitosamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpGet("balance")]
    public async Task<IActionResult> GetMyBalance()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue("id"));
            var walletDto = await _walletService.GetWalletByUserIdAsync(userId);
            return Ok(walletDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error al obtener el saldo.", error = ex.Message });
        }
    }

    [HttpPost("send-money")]
    public async Task<IActionResult> SendMoney([FromBody] SendMoneyRequestDto request)
    {
        try
        {
            var senderUserId = int.Parse(User.FindFirstValue("id"));
            var transactionDto = await _walletService.SendMoneyAsync(
                senderUserId,
                request.RecipientPhoneNumber,
                request.Amount,
                request.Description
            );
            return Ok(transactionDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error inesperado al procesar la transferencia.", error = ex.Message });
        }
    }

    [HttpPut("accept-transaction")]
    public async Task<IActionResult> AcceptTransaction([FromBody] AcceptTransactionRequestDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue("id"));
            await _walletService.AcceptTransactionAsync(userId, request.TransactionId);
            return Ok(new { message = "Transacción aceptada exitosamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetTransactionHistory()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue("id"));
            var history = await _walletService.GetTransactionHistoryAsync(userId);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error al obtener el historial de transacciones.", error = ex.Message });
        }
    }
}