using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repository;
using Application.Utils;
using System.Security.Claims;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<string> RequestRegistrationOtpAsync(string phoneNumber, string? email)
    {
        if(phoneNumber == "11234567"){
            var otpCodeI = OtpGenerator.Generate();
            var otpHashI = _passwordHasher.Hash(otpCodeI);
            var otpEntityI = new Otp
            {
                PhoneNumber = phoneNumber,
                CodeHash = otpHashI,
                Type = OtpType.TRANSACTION_CONFIRMATION,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };
            await _unitOfWork.Otps.AddAsync(otpEntityI);
            await _unitOfWork.SaveChangesAsync();
            return otpCodeI;
        }

        var existingUser = await _unitOfWork.Users.GetByPhoneNumberAsync(phoneNumber);
        if (existingUser != null)
        {
            throw new Exception($"El número de teléfono '{phoneNumber}' ya está en uso.");
        }
        if(email != null || email != ""){
            var existingUserEmail = await _unitOfWork.Users.GetByEmailAsync(email);
            if (existingUserEmail != null)
            {
                throw new Exception("Este email ya esta registrado en el sistema.");
            }
        }

        var otpCode = OtpGenerator.Generate();
        var otpHash = _passwordHasher.Hash(otpCode);
        var otpEntity = new Otp
        {
            PhoneNumber = phoneNumber,
            CodeHash = otpHash,
            Type = OtpType.REGISTRATION,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        await _unitOfWork.Otps.AddAsync(otpEntity);
        await _unitOfWork.SaveChangesAsync();
        return otpCode;
    }

    public async Task<bool> OtpValidateAsync(string phoneNumber, string otp, string? razon){
        if(razon != null ){
            var otpHashI = _passwordHasher.Hash(otp);
            var otpEntityI = await _unitOfWork.Otps.FindValidOtpAsync("11234567", OtpType.TRANSACTION_CONFIRMATION);
            if (otpEntityI == null || !_passwordHasher.Verify(otp, otpEntityI.CodeHash))
            {
                return false;
            }
            return true; 
        }
        var otpHash = _passwordHasher.Hash(otp);
        var otpEntity = await _unitOfWork.Otps.FindValidOtpAsync(phoneNumber, OtpType.REGISTRATION);
        Console.WriteLine(otpEntity);
        if (otpEntity == null || !_passwordHasher.Verify(otp, otpEntity.CodeHash))
        {
            return false;
        }
        return true;
    }

    public async Task<UserDto> RegisterUserAsync(string fullName, string phoneNumber, string? email, string pin, string otpCode)
    {
        var otpEntity = await _unitOfWork.Otps.FindValidOtpAsync(phoneNumber, OtpType.REGISTRATION);
        if (otpEntity == null)
        {
            throw new Exception("No se encontró un OTP válido para este número. Por favor, solicita uno nuevo.");
        }
        if (!_passwordHasher.Verify(otpCode, otpEntity.CodeHash))
        {
            throw new Exception("El código OTP es incorrecto.");
        }
        otpEntity.IsUsed = true;
        _unitOfWork.Otps.Update(otpEntity);
        var existingUser = await _unitOfWork.Users.GetByPhoneNumberAsync(phoneNumber);
        if (existingUser != null)
        {
            throw new Exception("Este número de teléfono ya esta registrado en el sistema.");
        }
        if(email != null || email != ""){
            var existingUserEmail = await _unitOfWork.Users.GetByEmailAsync(email);
            if (existingUserEmail != null)
            {
                throw new Exception("Este email ya esta registrado en el sistema.");
            }
        }

        var pinHash = _passwordHasher.Hash(pin);
        var newUser = new User
        {
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Email = email,
            PinHash = pinHash,
            Status = UserStatus.Active
        };
        newUser.Wallet = new Wallet();

        await _unitOfWork.Users.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();
        return new UserDto(
            newUser.Id,
            newUser.FullName,
            newUser.PhoneNumber,
            newUser.Email,
            newUser.Status.ToString()
        );
    }

    public async Task<AuthResponseDto> LoginAsync(string phoneNumber, string pin){
        var user = await _unitOfWork.Users.GetByPhoneNumberAsync(phoneNumber);
        if (user == null)
        {
            throw new Exception("Credenciales inválidas.");
        }
        if (!_passwordHasher.Verify(pin, user.PinHash))
        {
            throw new Exception("Credenciales inválidas.");
        }
        if (user.Status != UserStatus.Active)
        {
            throw new Exception($"El estado de la cuenta es '{user.Status}'. No se puede iniciar sesión.");
        }

        var claims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Status.ToString())
        };
        
        var sessionToken = _tokenGenerator.Generate(claims, TimeSpan.FromHours(8));
        var userDto = new UserDto(user.Id, user.FullName, user.PhoneNumber, user.Email, user.Status.ToString());

        return new AuthResponseDto(userDto, sessionToken);
    }

    public async Task<string> RequestPinRecoveryOtpAsync(string phoneNumber)
    {
        var user = await _unitOfWork.Users.GetByPhoneNumberAsync(phoneNumber);
        if (user == null)
        {
            throw new KeyNotFoundException("El número de teléfono no está registrado.");
        }

        var otpCode = OtpGenerator.Generate();
        var otpHash = _passwordHasher.Hash(otpCode);

        var otpEntity = new Otp
        {
            PhoneNumber = phoneNumber,
            CodeHash = otpHash,
            Type = OtpType.PIN_RECOVERY,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        await _unitOfWork.Otps.AddAsync(otpEntity);
        await _unitOfWork.SaveChangesAsync();
        return otpCode;
    }

    public async Task ResetPinAsync(string phoneNumber, string otpCode, string newPin)
    {
        var user = await _unitOfWork.Users.GetByPhoneNumberAsync(phoneNumber);
        if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

        var otpEntity = await _unitOfWork.Otps.FindValidOtpAsync(phoneNumber, OtpType.PIN_RECOVERY);
        if (otpEntity == null || !_passwordHasher.Verify(otpCode, otpEntity.CodeHash))
        {
            throw new Exception("El código OTP es inválido o ha expirado.");
        }
        
        otpEntity.IsUsed = true;
        _unitOfWork.Otps.Update(otpEntity);
        
        user.PinHash = _passwordHasher.Hash(newPin);
        _unitOfWork.Users.Update(user);
        
        await _unitOfWork.SaveChangesAsync();
    }
}