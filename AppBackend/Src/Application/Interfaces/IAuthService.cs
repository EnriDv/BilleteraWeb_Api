using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<string> RequestRegistrationOtpAsync(string phoneNumber, string? email);
    Task<bool> OtpValidateAsync(string phoneNumber, string otp, string? razon);
    Task<UserDto> RegisterUserAsync(string fullName, string phoneNumber, string? email, string pin, string otpCode);
    Task<AuthResponseDto> LoginAsync(string phoneNumber, string pin);
    Task<string> RequestPinRecoveryOtpAsync(string phoneNumber);
    Task ResetPinAsync(string phoneNumber, string otpCode, string newPin);
}