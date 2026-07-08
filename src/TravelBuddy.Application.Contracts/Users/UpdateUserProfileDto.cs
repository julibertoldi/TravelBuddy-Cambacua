using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelBuddy.Users
{
    public class UpdateUserProfileDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public string? Preferencias { get; set; }
    }
}
