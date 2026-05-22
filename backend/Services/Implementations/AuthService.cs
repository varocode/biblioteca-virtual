using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs.Auth;
using BibliotecaAPI.DTOs.Usuarios;
using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;
using BibliotecaAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaAPI.Services.Implementations;

public class AuthService(BibliotecaContext context, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponseDto> RegistrarAsync(RegisterRequestDto request)
    {
        var email = NormalizarEmail(request.Email);
        if (await context.Usuarios.AnyAsync(usuario => usuario.Email == email))
        {
            throw new InvalidOperationException("Ya existe un usuario con ese email.");
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = RolUsuario.Lector,
            Telefono = NormalizarOpcional(request.Telefono),
            Direccion = NormalizarOpcional(request.Direccion),
            FechaRegistro = DateTime.UtcNow,
            Activo = true
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return CrearRespuesta(usuario);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = NormalizarEmail(request.Email);
        var usuario = await context.Usuarios.SingleOrDefaultAsync(usuario => usuario.Email == email);
        if (usuario is null || !usuario.Activo || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        return CrearRespuesta(usuario);
    }

    public async Task<UsuarioDto?> ObtenerUsuarioActualAsync(int usuarioId)
    {
        var usuario = await context.Usuarios.AsNoTracking().SingleOrDefaultAsync(usuario => usuario.Id == usuarioId && usuario.Activo);
        return usuario is null ? null : UsuarioDto.DesdeEntidad(usuario);
    }

    private AuthResponseDto CrearRespuesta(Usuario usuario)
    {
        var expiraEn = DateTime.UtcNow.AddHours(configuration.GetValue("Jwt:ExpirationHours", 8));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no está configurado.");
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiraEn,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEn = expiraEn,
            Usuario = UsuarioDto.DesdeEntidad(usuario)
        };
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
    private static string? NormalizarOpcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
