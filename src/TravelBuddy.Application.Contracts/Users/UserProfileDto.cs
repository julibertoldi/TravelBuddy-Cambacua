using System;

namespace TravelBuddy.Users;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FotoPerfilUrl { get; set; }
    public string? Preferencias { get; set; }
}
