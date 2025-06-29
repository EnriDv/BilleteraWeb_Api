using Application.Interfaces;
using InterfaceAdapters.DTO.Auth;
using Microsoft.AspNetCore.Mvc;

namespace InterfaceAdapters.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestRegistrationOtp([FromBody] RequestOtpRequestDto request)
    {
        try
        {
            var otp = await _authService.RequestRegistrationOtpAsync(request.PhoneNumber, request.Email);
            return Ok(new { message = "OTP generado.", otpCode = otp });
        }
        catch (Exception ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpPost("validate-otp")]
    public async Task<IActionResult> OtpValidate([FromBody] RequestOtpValidateDto request)
    {
        try
        {
            if(request.PhoneNumber == "11234567"){
                var validateI = await _authService.OtpValidateAsync(request.PhoneNumber, request.OtpCode, "pending");
                return Ok(validateI);  
            }
            var validate = await _authService.OtpValidateAsync(request.PhoneNumber, request.OtpCode, null);
            return Ok(validate);
        }
        catch (Exception ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request)
    {
        try
        {
            var userDto = await _authService.RegisterUserAsync(
                request.FullName,
                request.PhoneNumber,
                request.Email,
                request.Pin,
                request.OtpCode
            );
            return StatusCode(201, new { message = "Usuario registrado exitosamente.", user = userDto });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var authResponse = await _authService.LoginAsync(request.PhoneNumber, request.Pin);
            return Ok(new 
            { 
                message = "Inicio de sesión exitoso",
                user = authResponse.User,
                token = authResponse.Token
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-pin/request-otp")]
    public async Task<IActionResult> RequestPinRecovery([FromBody] RequestOtpRequestDto request)
    {
        try
        {
            var otp = await _authService.RequestPinRecoveryOtpAsync(request.PhoneNumber);
            return Ok(new { message = "OTP de recuperación generado.", otpCode = otp });
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

    [HttpPost("forgot-pin/reset")]
    public async Task<IActionResult> ResetPin([FromBody] ResetPinRequestDto request)
    {
        try
        {
            await _authService.ResetPinAsync(request.PhoneNumber, request.OtpCode, request.NewPin);
            return Ok(new { message = "Tu PIN ha sido restablecido exitosamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}