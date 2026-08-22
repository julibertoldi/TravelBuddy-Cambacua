using System;
using TravelBuddy.Destinations;
using Volo.Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
namespace TravelBuddy.Favorites;
public class Favorite : Entity
{
    public Guid UsuarioId { get; private set; }
    public Guid DestinoId { get; private set; }
    [ForeignKey(nameof(DestinoId))]
    public virtual Destination Destination { get; private set; }

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