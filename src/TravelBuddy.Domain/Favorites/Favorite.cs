using System;
using Volo.Abp.Domain.Entities;
namespace TravelBuddy.Favorites;
public class Favorite : Entity
{
    public Guid UsuarioId { get; private set; }
    public Guid DestinoId { get; private set; }
 
    private Favorite() { }
    public Favorite(Guid usuarioId, Guid destinoId)
    {
        UsuarioId = usuarioId;
        DestinoId = destinoId;
    }
    public override object[] GetKeys()
    {
        return new object[] { UsuarioId, DestinoId };
    }
}