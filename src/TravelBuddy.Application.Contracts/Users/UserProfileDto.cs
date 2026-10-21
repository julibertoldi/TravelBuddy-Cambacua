using System;
using System.ComponentModel.DataAnnotations;

namespace TravelBuddy.Users;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;

    [Required, StringLength(64)]
    public string Nombre { get; set; } = string.Empty;
    
    [Required, StringLength(64)]
    public string Apellido { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    public string? FotoPerfilUrl { get; set; }
    public string? Preferencias { get; set; }
}
