using Dapper;
using clinic.Data;
using clinic.DTOs.Auth;
using clinic.Helpers;
using clinic.Models;
using clinic.Services.Interfaces;

namespace clinic.Services
{
    public class AuthService : IAuthService
    {
        private readonly DapperContext _context;
        private readonly JwtHelper _jwt;

        public AuthService(DapperContext context, JwtHelper jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            using var db = _context.CreateConnection();
            var user = await db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email=@Email AND Status='Active'",
                new { dto.Email });

            if (user == null) return null;
            if (!PasswordHelper.Verify(dto.Password, user.Password)) return null;

            return new AuthResponseDto
            {
                Token = _jwt.GenerateToken(user),
                Name = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            using var db = _context.CreateConnection();

            var exists = await db.QueryFirstOrDefaultAsync<User>(
                "SELECT Id FROM Users WHERE Email=@Email", new { dto.Email });
            if (exists != null) return false;

            await db.ExecuteAsync(@"
                INSERT INTO Users (FullName, Email, Phone, Password, Role)
                VALUES (@FullName, @Email, @Phone, @Password, @Role)",
                new
                {
                    dto.FullName,
                    dto.Email,
                    dto.Phone,
                    Password = PasswordHelper.Hash(dto.Password),
                    Role = "Receptionist" // Hardcoded — never trust a role sent by the client
                });
            return true;
        }
    }
}