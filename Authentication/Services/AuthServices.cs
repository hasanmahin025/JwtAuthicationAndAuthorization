using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentication.Data;
using Authentication.Entities;
using Authentication.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Services;

public class AuthServices : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly MyDbcontext _dbcontext;

    public AuthServices(IConfiguration configuration, MyDbcontext dbcontext)
    {
        _configuration = configuration;
        _dbcontext = dbcontext;
    }

    public async Task<Users?> RegisterAsync(UserDto request)
    {
       
        if (await _dbcontext.Users.AnyAsync(u => u.UserName == request.UserName))
            return null;

        var user = new Users
        {
            UserName = request.UserName,
            PasswordHash = new PasswordHasher<Users>()
                .HashPassword(null!, request.Password) 
        };

        _dbcontext.Users.Add(user);
        await _dbcontext.SaveChangesAsync();

        return user;
    }

    public async Task<TokenResponseDto?> LoginAsync(UserDto request)
    {
        Users? user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);

        if (user == null)
            return null;

        var verifyResult = new PasswordHasher<Users>()
            .VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verifyResult == PasswordVerificationResult.Failed)
            return null;
        var token = new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshToken(user)

        };

        return token;
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenDto request)
    {
        var _user = await _dbcontext.Users.FindAsync(request.UserId);
        if (_user == null || _user.RefreshToken != request.RefreshToken
                          || _user.RefreshTokenExpiration < DateTime.UtcNow)
            return null;
        var token = new TokenResponseDto
        {
            AccessToken = CreateToken(_user),
            RefreshToken = await GenerateAndSaveRefreshToken(_user)
        };

        return token;
    }

    private async Task<string> GenerateAndSaveRefreshToken(Users user)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        
        var refreshToken = Convert.ToBase64String(randomNumber);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(7);
        await _dbcontext.SaveChangesAsync();
        return refreshToken;
        

    }

    private string CreateToken(Users user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
            
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}

public interface IAuthService
{
    Task<Users?> RegisterAsync(UserDto request);
    Task<TokenResponseDto?> LoginAsync(UserDto request);
    Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenDto request);
}
