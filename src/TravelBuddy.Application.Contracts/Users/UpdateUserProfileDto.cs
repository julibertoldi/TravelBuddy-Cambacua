using System.ComponentModel.DataAnnotations;

public class UpdateUserProfileDto
{
    [Required, StringLength(64)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(64)]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(512)]
    public string? FotoPerfilUrl { get; set; }

    [StringLength(2000)]
    public string? Preferencias { get; set; }
}